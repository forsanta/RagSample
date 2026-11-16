# RagSample : le RAG en hello world .NET

Un exemple minimal de **RAG** (*Retrieval-Augmented Generation*) en C#, sans framework ni package NuGet : un `HttpClient`, Ollama, et environ 70 lignes de code.

Le programme part de quatre messages d'un chat d'entreprise. Il pose une question qui oblige le LLM à **croiser deux messages** pour répondre :

> Je peux venir en voiture et me garer au bureau pour la réunion d'équipe ?

## Les 3 étapes

1. **R (Retrieval)** : chaque message est transformé en vecteur (embedding) avec `nomic-embed-text`. On garde les 2 messages les plus proches de la question (similarité cosinus).
2. **A (Augmented)** : ces 2 messages sont ajoutés au prompt, juste avant la question.
3. **G (Generation)** : `gemma4:e2b` lit les messages et rédige la réponse.

Le programme affiche aussi la réponse **sans RAG**, pour comparer.

## Lancer l'exemple

Prérequis : le [SDK .NET 11](https://dotnet.microsoft.com/download) et [Ollama](https://ollama.com).

```bash
ollama pull nomic-embed-text
ollama pull gemma4:e2b
dotnet run
```

Résultat obtenu :

```
0,74  Julie : la réunion d'équipe a lieu vendredi à 9h30.
0,65  Marc : le parking du bureau est réservé aux visiteurs le vendredi.
0,60  Julie : la machine à café du 2e étage est en panne.
0,58  Thomas : le mot de passe du Wi-Fi du bureau est SOLEIL-2026.

Sans RAG : Veuillez vérifier les règles de stationnement du bureau pour confirmer si cela est autorisé.

Avec RAG : Le parking du bureau est réservé aux visiteurs le vendredi.
```

Les réponses peuvent varier un peu selon la version du modèle.

Pour changer de modèle, modifiez les constantes `ModeleEmbedding` et `ModeleChat` en haut de `Program.cs`.
