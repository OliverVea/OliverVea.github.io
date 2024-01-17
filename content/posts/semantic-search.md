---
title: "Introduction to Semantic Search"
date: "2023-12-20"
author: "Oliver Vea"
tags:
    - Theory
    - Semantic Search
---

## Overview

Recently, I've been researching semantic search, since elasticsearch 8 has added support for doing vector search as part of an ordinary search request.

Semantic search is a searching algorithm that builds on the rapid progress made in NLP with deep learning methods. It is possible to get high-dimensional vectors, called embeddings, which approximate the semantic meaning of any text.

To leverage this for searching, all the searchable documents can be embedded with this semantic vector. When a user provides a search query, this query is also embedded. The query embedding can then be compared with the document embeddings with a [similarity metric](https://www.elastic.co/guide/en/elasticsearch/reference/current/dense-vector.html#dense-vector-similarity) to get a list of documents by relevance.

This is usually done with a k-nearest-neighbor, `knn`, or approximate nearest neighbor, `ann`, algorithm, to get the `k` most relevant search results.

[![Semantic search illustration from elastic.co](https://images.contentstack.io/v3/assets/bltefdd0b53724fa2ce/bltf137a833984d3581/63728faba32209106e8b0b72/vector-search-diagram.png)](https://www.elastic.co/what-is/semantic-search)

There are multiple benefits with using semantic search over traditional lexical search.

### Semantic meaning over string comparison

With traditional search, synonyms and other linguistic relationships between words are ignored. If you are searching for a `citrus scent`, you will not get results on `lemon scent` or `orange scent`, despite both lemons and oranges being in the category of citrus fruits.

With semantic search, however, the embedding will contain the semantic meaning of the phrase and documents. The embeddings for `citrus`, `lemon` and `orange` will be close, and therefore the similarity will be high for the search phrase and relevant documents.

### Multimodal documents

With traditional search it is impossible to do comparisons to binary data such as audio, images and video. The binary data has to be transcribed to text for them to be comparable. For semantic search, as long as a model is available, in principle any kind of data can be transformed into an embedding and be compared in the embedding space.

This means that product images, videos and audio clips can be searched as well as text. It might be possible to search for `melancholy blues song` and get relevant song suggestions, without the song ever being described in text.

### Multimodal queries

Of course, the opposite is also possible. For example, for fashion websites, it is possible for users to upload images of an outfit. The search results might then contain the items in the outfit or items that might be similar to or in the same style as the provided image.

### Multilingual search

Lastly, it is possible to do multilingual searching. As a Danish search engineer, the combination of English and Danish product descriptions and search queries are a real problem that lexical search has a hard time addressing due to, e.g., monolingual stemmers.

With semantic search, it is possible to train embedding models to embed from multiple languages, allowing the models to understand multiple languages and extract the semantic meaning into the same vector representations.
