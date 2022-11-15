---
title: "veachron"
date: "2022-11-13"
author: "Oliver Vea"
---

## Overview

Veachron is an application which can help storing and displaying timings. The main use-case is for debugging, as it can be embedded into code and used to get actual timings. The project is freely available [on Github](https://github.com/OliverVea/veachron).

The project consists of a Python application, using the [__Flask__](https://flask.palletsprojects.com/en/2.2.x/) framework to host a [__REST API__](https://restfulapi.net/). The API is documented through [__swagger__](https://swagger.io/), to ensure ease of use.

The application can be hosted directly, but alternatively, two [__Docker__](https://hub.docker.com/repository/docker/olivervea/veachron-ui) [__images__](https://hub.docker.com/repository/docker/olivervea/veachron-api) are provided, with the application and a [__PostgreSQL__](https://hub.docker.com/_/postgres) database, for storing the timer logs.

A test suite with [__end to end tests__](https://github.com/OliverVea/veachron/blob/main/tests/test_application/test_timing.py) covering the entire functionality of the application has been added to the project.

To ensure that changes to the code do not effect existing functionality, the entire test suite is run on all pull requests, and it is required that all tests pass before the pull request can be merged. This is achieved using [__Github Actions__](https://github.com/OliverVea/veachron/actions).

A release pipeline has been added to push the application to [__PyPi__](https://test.pypi.org/project/veachron/) and [__Docker hub__](https://hub.docker.com/u/olivervea).

To display the timings in a visually easy-to-understand manner, a [__React__](https://reactjs.org/) application has been written. The UI can be hosted with [__Node.js__](https://nodejs.org/en/) and allows the user to have a high level of granularity by expanding and minimizing individual timing nodes.

Veachron is mostly a demonstration project for showing some of the skills I've picked up, while working at hesehus. The usefulness of the application itself is highly questionable, as it is probably completely unable to do precise timings without adding to the measurements by requiring the application to wait for network requests.

## Installation

Two options for running veachron is provided. Firstly, it can be installed with PyPi and directly run with Python. Alternatively, a docker image is available and can be used to host veachron in docker.

As veachron requires a PostSQL database for storing timings, a PostgreSQL connection has to be configured in veachron. This can be configured with the following environment variables: 

* `DB_HOST` specifying the host address for the database.
* `DB_USER` specifying the user for connecting to the database.
* `DB_PASSWORD` specifying the password for connecting to the database.

It is vital that a database with the name `veachron` exists in the PostgreSQL server and that the user credentials provided to veachron has access to this database.

For ease of use, the Docker method of hosting is recommended, whenever Docker is available.

### Direct Hosting

To run veachron with Python directly, it has been pushed to the [PyPi test server](https://test.pypi.org/project/veachron/). This means it can be installed with pip using the command `pip install -i https://test.pypi.org/simple/ veachron`.

It can then be run with Python using the command `python -m veachron`.

> Currently, direct hosting does not support running the UI.

### Prebuilt Docker images

The simplest way to run veachron is using the publically available docker images from [Docker hub](https://hub.docker.com/r/olivervea/veachron). 

The following docker-compose file shows how to use the image and configure it to use a PostgreSQL database, also hosted with Docker:

```yaml
services:
  api:
    image: olivervea/veachron-api:latest
    environment:
      DB_HOST: db
      DB_USER: user
      DB_PASSWORD: password
      LOG_LEVEL: WARNING

  ui:
    image: olivervea/veachron-ui:latest

  db:
    image: postgres
    restart: always
    environment: 
      POSTGRES_USER: user
      POSTGRES_PASSWORD: password
      POSTGRES_DB: veachron
    volumes:
    - postgresql:/var/lib/postgresql/data

  nginx:
    image: nginx:1.21
    platform: linux
    ports:
      - 80:80
    restart: on-failure
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf

volumes:
  postgresql:
```

The docker-compose file includes 4 services: the veachron API and UI, the PostgreSQL database and Nginx for nicer routing between the three. 

> Due to issues with configuring the React app which hasn't been solved, the current Nginx routing configuration is the only configuration supported by the UI.

The following file shows the Nginx configuration:

```nginx
events {

}

http {
  resolver 127.0.0.11 ipv6=off valid=1s;

  proxy_set_header Host $host;
  proxy_set_header X-Forwarded-For $remote_addr;
  proxy_set_header X-Forwarded-Proto $scheme;

  server {
    listen 80;
    server_name ui.localhost;

    location / {
      set $backend "http://ui:3000";
      proxy_pass $backend;
    }
  }

  server {
    listen 80;
    server_name api.localhost;

    location / {
      set $backend "http://api:5000";
      proxy_pass $backend;
    }
  }

  server {
    listen 80 default_server;
    server_name _;

    large_client_header_buffers 4 16k;

    return 404;
  }
}
```

This file is referenced in the docker-compose.yaml file, and must therefore be available, so the Nginx routing is configured. The file structure should resemble the following:

```
└── Parent Folder
    ├── docker-compose.yml
    └── nginx.conf
```

To run veachron, navigate inside the parent folder and run `docker compose up -d`.

### Docker images from source

As veachron is completely open source, it is also possible to check out the [Github repository](https://github.com/OliverVea/veachron) and build the Docker images locally.

This allows a user to change the source code of veachron and configure to ones hearts content, as well as run the tests locally.

Simply follow the readme.md file in the repository root for instructions on how to build and run the Docker images from source code.

## Using veachron

After running the application, the UI can be found at [http://ui.localhost](http://ui.localhost), and the API can be found at [http://api.localhost](http://api.localhost).

### Using the API

When accessing the API in a browser, it automatically redirects to the swagger documentation, where the API can be tinkered with.

The idea of the API currently rests on two collections: the `timers` collection, and a `timings` collection for each `timer`.

The `timer` marks some code segment to be timed. It has an `entry` and an `exit`. A `timer` can also have a `parentId`, denoting that the timer is inside of another timer, so a ratio of how much time is spent is specific segments of the code block can be calculated and displayed.

Whenever the `entry` of a timer is reached, a `timers/{timerId}/add-entry` post request is made, to register the time of the entry. This instantiates a `timing` in the `timings` collection of the `timer`.

> It is highly recommended to use a client-side `timestamp`, as veachron will resort to a server-side `timestamp`, if none is provided.

When the corresponding `exit` of the `timer` is reached, a `timers/{timerId}/timings/{timingId}/add-exit` post request can then be made to complete the `timing`, adding it to the total time spent of the `timer`.

The endpoint `timers/list-timings` can be used to see a tree of all `timers` and a breakdown of the time spendeture for each.
