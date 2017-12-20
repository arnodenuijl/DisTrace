# DisTrace

Distributed Tracing in .net

## Purpose

Writing distributed systems is hard. Trying to figure out what is happening/has happened in a distributed system is much harder. 
One well known way to facilitate in this is the usage of correlation id's. The general idea being that by using a (the same) correlation id across different linked service calls and using them in e.g. logging you can figure out what calls are related. This library helps with that by facilitating the usage of correlation and causation ids with which you can exactly see which calls are related and who invoked who.

## Why this library?

There are a lot of ways to do this and a lot of libraries to help you with this. Why write this one? The main reason is that, for now at least, I have a very simple and focused target. A lot of libraries provide more functionality that I’m just not ready for.

## What’s the concept?

The use case is a system build from multiple parts that call each other. E.g. a microservice based system where different service call each other’s API or consume each other’s events. From there we do two things. We want to track calls and message’s through the system and correlate them. We do that by introducing three ids.

* Request id: Every request/call/unit of work in a service gets its own unique request id. If you e.g. have an API with a /api/random then every call to that API gets a unique request id that is only valid for the duration of that specific request

* Causation id: If a service is called and the caller sends it’s own request id with the request, than the callee can use that as it’s causation id.

* Correlation id: While calls that are related to each other can be linked with each other with only the request and causation ids, it’s convenient to also have a correlation id. This is an id that is the same for all the calls in all the services within one single conversation.

If every component for every request (or other unit of work)  tracks it’s request, causation and correlation ids then we can use them in logging.

# What does this library do?

There is no magic in this all. It’s only a question of doing it, and doing it consequently. This library provides a couple of things to help.

## The core

* **DisTrace.Core** provides a small set of classes that are used in every situation. Mainly the `TracingContext` class to hold the request, causation and correlation id. And a `ITracingContextProvider` interface that defines methods to put or get the TracingContext on a request/unit of work

## For asp.net core

* **DisTrace.AspNetCore** provides a middleware that can be used to get request and correlation ids from incoming requests, create a `TracingContext` and put them on the request context
* **DisTrace.AspNetCore.SeriLog** provides a middleware that can be used to get the `TracingContext` from the request context and puts is on the SeriLog logcontext for the scope of the request. This ensures that with everything that is logged, the `TracingContext` is included in the log message

## For WebApi 2

* **DisTrace.WebApi** provides a `DelegatingHandler` implementation that can be used to get request and correlation ids from incoming requests, create a `TracingContext` and put them on the request context using the `HttpRequestTracingContextProvider`
* **DisTrace.WebApi.SeriLog** provides a `DelegatingHandler` implementation that can be used to get the `TracingContext` from the request context and puts is on the SeriLog logcontext for the scope of the request. This ensures that with everything that is logged, the `TracingContext` is included in the log message

## HttpClient

* **DisTrace.HttpClient** provides `AddTracingContextToRequestHandler`. A `DelegatingHandler` that can be used in a HttpClient. The `AddTracingContextToRequestHandler` takes a `ITracingContextProvider` to get access to the current `TracingContext` when doing an Http call. 



TODO: Elaborate & examples
For now, look at the `AspNetCoreIntegrationTest` and the `WebApiIntegrationTest` 
