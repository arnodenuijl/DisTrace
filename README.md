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

If every call tracks it’s request, causation and correlation ids then we can use them in logging.

# What does this library do?

There is no magic in this all. It’s only a question of doing it, and doing it consequently.

* Core provides a small set of classes to help doing it consequently.
* TracingContext: A class to hold the request, causation and correlation id
* ITracingContextProvider: An interface for getting and setting a tracing context. E.g. in a web app you would use an implementation to get and set it on the request context. For a message based system you would make an implementation to get and set the tracing context on the scope of the message handler.
* SingleTracingContextProvider: A simple implementation of the ITracingContextProvider that just stores it in a field. 
* TracingContextHeaders: Headers that can be used to store the id’s in e.g. http headers.
* ......

TODO: Elaborate

