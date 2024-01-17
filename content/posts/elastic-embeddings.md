---
title: "Semantic Search with OpenAI and Elasticsearch"
date: "2023-12-27"
author: "Oliver Vea"
tags:
    - Project
    - Semantic Search
---

## Overview

Recently, I've been researching semantic search, since elasticsearch 8 has added support for doing vector search as part of an ordinary search request. I've written [a post](../semantic-search) about this topic, providing an introduction to the uninitiated.

I wanted to build a simple semantic search proof-of-concept project with [OpenAI embeddings](https://platform.openai.com/docs/guides/embeddings) and searching with the [elasticsearch `knn` option](https://www.elastic.co/guide/en/elasticsearch/reference/current/knn-search.html).

The result is this small project with an ASP.NET API, which allows for product ingestion and searching. The project is freely available [on Github](https://github.com/OliverVea/ElasticEmbeddings).

The application is intended to run in docker, and for the sake of convenience, a [docker-compose.yml](https://github.com/OliverVea/ElasticEmbeddings/blob/main/docker-compose.yml) file is provided, containing an Elasticsearch instance and the application itself.

A [Dockerfile](https://github.com/OliverVea/ElasticEmbeddings/blob/main/Dockerfile) allows a docker image to be built from the source code.

The application keeps track of the state of the documents. When a new document is created or an existing document is changed, it is put in the `Created` state, waiting to be embedded in the OpenAI API. When it has been embedded it is put in the `Embedded` state, waiting to be indexed in elasticsearch. When it has been indexed, it is put in the `Indexed` state, and is available for searching.

The program automatically processes the ingested documents, meaning that there is no needlessly big response time either in the ingestion or search endpoints.

## Using ElasticEmbeddings

To run the application, follow these steps:

1. Ensure that you have a valid Azure OpenAI API key
1. Rename the `.env.template` file to `.env` and fill in the values
1. Run the application with `docker-compose up -d`
1. The API is now available at <http://localhost:8080/swagger/index.html>

To start using the application, data will have to be ingested with the `POST /documents` or `PUT /documents/{id}` endpoints. The `POST` endpoint will return a document id and the `PUT` endpoint can be used to create a new document with a specific id or to update an existing one.

The `GET /documents` endpoint can be used to get all ingested document ids.

Lastly, after waiting for a few seconds, the documents are searchable with the `GET search/{query}` endpoint. As there is currently no support for filtering or other features other than just a search phrase, it is implemented as a `GET` method.

## Demonstration

A [demonstration IPython notebook](https://github.com/OliverVea/ElasticEmbeddings/blob/main/demo.ipynb) has been added to show how to use the application in Python.

The example creates a list of books:

```python
# Creating a list of Book objects with titles and GUIDs
book_titles = [
    ('The Fellowship of the Ring', 'c4b50524-b9b3-4942-9306-434468277362'),
    ('The Two Towers', '48bc5d77-ec23-401b-8d85-5d93fd312f78'),
    ('The Return of the King', 'b63d2123-74bb-41da-9e35-d2ba2a7e9d3b'),
    ('The Lion, the Witch and the Wardrobe', 'e369d1c5-e052-48d0-aec6-497e8271443b'),
    ('Harry Potter and the Philosopher\'s Stone','31482626-c6cb-4a31-b024-fe49c6902f38'),
    ('Harry Potter and the Chamber of Secrets', 'd088ce5b-ff55-4c5f-a67d-ad33fa77e8e1'),
    ('Harry Potter and the Prisoner of Azkaban', '604b6242-1723-4ccb-9ed6-91d3c39500cc'),
    ('Harry Potter and the Goblet of Fire', 'e576f648-b440-43b3-98fe-abccd2311346'),
    ('Harry Potter and the Order of the Phoenix', '5bd8420d-adbd-4279-b23d-7aea2341ed5e'),
    ('Harry Potter and the Half-Blood Prince', '103afa59-919f-42e7-a9e3-1c94b7e825c8'),
    ('Harry Potter and the Deathly Hallows', '4cf5cbf7-4421-4895-8b3a-e255fe9604f5'),
]
```

It then searches on various phrases, and gets answers which are impossible to get with traditional lexical search.

```python
search_books('JK Rowling')
> [("Harry Potter and the Philosopher's Stone", 0.92648256),
 ('Harry Potter and the Order of the Phoenix', 0.9242429),
 ('Harry Potter and the Deathly Hallows', 0.92275167),
 ('Harry Potter and the Half-Blood Prince', 0.9210466),
 ('Harry Potter and the Prisoner of Azkaban', 0.91992533)]

search_books('JRR Tolkien')
> [('The Fellowship of the Ring', 0.9275134),
 ('The Two Towers', 0.92282647),
 ('The Return of the King', 0.916716),
 ("Harry Potter and the Philosopher's Stone", 0.9129532),
 ('The Lion, the Witch and the Wardrobe', 0.9113982)]

search_books('Gandalf')
> [('The Fellowship of the Ring', 0.91779804),
 ('The Two Towers', 0.90987486),
 ('The Return of the King', 0.90977865),
 ('Harry Potter and the Goblet of Fire', 0.9081961),
 ('Harry Potter and the Deathly Hallows', 0.90047026)]
```

