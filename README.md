# Hacker News Stories API

## Overview

This is a simple solution for the Santander Developer Coding Test. It uses Asp.Net Core Web API components. It is built with .NET 7.0 as a target framework.

It exposes a single uri at http://localhost:5267/api/stories/{count}, to return the top _count_ stories as exposed by Hacker News (https://github.com/HackerNews/API).

## Implementation Approach

The core task was sraight-forward enough. The key consideration was how to support the ability **to efficiently service large numbers of requests without risking overloading of the Hacker News API**.  
To this end, I chose to use an in-memory cache for caching the full list of (up to 500) best Hacker News story summaries for an expiration time of 10 minutes.
If the cache is already populated while servicing a request, the N best stories are read from the cache instead, otherwise the full list of best stories is fetched from Hacker News. To avoid a potential collision of multiple simultaneous (dirty/inconsistent) reads, as well as potential multiple fetches from the Hacker News API, the cache is synchronised with a semaphore.
That way, only one thread/request at a time will be responsible for fetching from Hacker News and caching.

## Assumptions

There is no implementation of **Authentication and authorisation** on the API. For a fully-featured API this would have to be implemented somehow.
Other considerations would have to be made about such important issues like hosting, scaling, API management, health monitoring, etc.

## To Run

1. Clone the project using your favourite git client or on the command terminal.
1. You should have .NET 7.0 installed. Build using your favourite IDE (I used JetBrains Rider 2023.2.2). Be sure to update all the nuget packages.
1. Run the unit tests from the IDE
1. Run the API from the IDE
This should launch the swagger page in the browser. Type the API url in the browser (e.g http://localhost:5267/api/stories/10). The requested list of best stories should show in the browser.

