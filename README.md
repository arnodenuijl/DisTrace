# DisTrace

Distributed Tracing in .net

## Purpose

Writing distributed systems is hard. Trying to figure out what is happening/has happened in a distributed system is much harder. 
One well known way to facilitate in this is the usage of correlation id's. The general idea being that by using a (the same) correlation id across different linked service calls and using them in e.g. logging you can figure out what calls are related. This library helps with that by facilitating the usage of three ids with which you can exactly see which calls are related and who invoked who.

## Why this library?

There are a lot of ways to do this and a lot of libraries to help you with this. Why write this one? The main reason is that, for now at least, I have a very simple and focused target. A lot of libraries provide more functionality that I’m just not ready for.

## What’s the concept?

The use case is a system build from multiple parts that call each other. E.g. a microservice based system where different service call each other’s API or consume each other’s events. From there we do two things. We want to track calls and message’s through the system and correlate them. We do that by introducing three concepts.

* `Unit of work id`: Every request/call/unit of work in a service gets its own unique id. If you e.g. have an API with an /api/something then every call to that API gets a unique id that is only valid for the duration of that specific request. E.g.via the standard x-request-id header. In this case the unit of work id is the request id. In other cases it it can be something that signifies a (batch) run or some other measure of the unit of work.

* `Causation id`: If a unit of work is caused by another unit of work it's good to store that information so later we can recreate the call graph. So the `causation id` is the `unit of work id` of the unit of work that called/issued this unit of work.

* `Flow id`: While calls that are related to each other can be linked with each other with only the unit of work and causation ids, it’s convenient to also have a flow id. This is an id that is the same for all the calls in all the services within one single conversation.

If every component for every request (or other unit of work)  tracks it’s unit of work, causation and flow ids then we can use them in logging and e.g. to build a graph of related calls.

# What does this library do?

There is no magic in this all. It’s only a question of doing it, and doing it consequently. This library provides a couple of things to help.

## The core

**DisTrace.Core** provides a small set of classes that are used in every situation. Mainly the `TracingContext`. The `TracingContext` has three properties

`UnitOfWorkId`: This identifies a unit of work. E.g. a request, the handling of a single event, a job that runs, etc. 

`CausationId`: If a unit of work is initiated by another unit of work the causation id can be used to track the initiator.

`FlowId`: The flow id identifies the bigger conversation or transaction that the unit of work belongs to.

The other thing in **DisTrace.Core** is the  `ITracingContextProvider` interface that defines methods to put or get the TracingContext on a unit of work. This can be implemented to get and set the TracingContext on a request, thread or other thing. 

## For asp.net core

* **DisTrace.AspNetCore** provides a middleware that can be used to get the ids from incoming requests, create a `TracingContext` and put the `TracingContext` on the request.

It does the following mapping:

Header | TracingContext property |
---|---|
X-Request-Id | UnitOfWorkId |
X-Causation-Id | CausationId |
X-Flow-Id | FlowId |

The unit of work id is retrieved from the `X-Request-Id` header. The reason the names are different is that the `X-Request-Id` is an often used header to provide a HTTP request with an id for the request. `TracingContext` can however also be used in other contexts then HTTP requests. And when the unit of work is a batch job run the name request id would be misleading.

* **DisTrace.AspNetCore.SeriLog** provides a middleware that gets the `TracingContext` from the request context and puts it on the SeriLog logcontext for the scope of the request. This ensures that with everything that is logged, the `TracingContext` is included in the log message

## For WebApi 2

* **DisTrace.WebApi** provides a `DelegatingHandler` implementation that can be used to read the headers of the incoming requests, create a `TracingContext` and put it on the request context using the `HttpRequestTracingContextProvider`. The headers and mapping to the `TracingContext` is the same as in `DisTrace.AspNetCore`.

* **DisTrace.WebApi.SeriLog** provides a `DelegatingHandler` implementation that can be used to get the `TracingContext` from the request context and puts is on the SeriLog logcontext for the scope of the request. This ensures that with everything that is logged, the `TracingContext` is included in the log message

## HttpClient

* **DisTrace.HttpClient** provides `AddTracingContextToRequestHandler`. A `DelegatingHandler` that can be used in a HttpClient. The `AddTracingContextToRequestHandler` takes a `ITracingContextProvider` to get access to the current `TracingContext` when doing an Http call. When a HTTP call is made it retrieves the current TracingContext and uses it to set headers on the HTTP request. The mapping from properties to headers is the same as in `DisTrace.AspNetCore`



TODO: Elaborate & examples
For now, look at the `AspNetCoreIntegrationTest` and the `WebApiIntegrationTest` 
