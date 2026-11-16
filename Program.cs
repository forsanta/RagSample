using System.Net.Http.Json;

// Les modèles Ollama utilisés
const string ModeleEmbedding = "nomic-embed-text"; // transforme un texte en vecteur
const string ModeleChat = "gemma4:e2b";             // rédige la réponse

using var http = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };

// Les messages du chat d'entreprise : des infos que le LLM ne peut pas connaître
string[] messagesChatEntreprise =
[
    "Thomas : le mot de passe du Wi-Fi du bureau est SOLEIL-2026.",
    "Julie : la réunion d'équipe a lieu vendredi à 9h30.",
    "Marc : le parking du bureau est réservé aux visiteurs le vendredi.",
    "Julie : la machine à café du 2e étage est en panne.",
];

// Aucun message ne contient la réponse telle quelle : il faut en croiser deux.
var question = "Je peux venir en voiture et me garer au bureau pour la réunion d'équipe ?";

// 1. RETRIEVAL : trouver les 2 messages les plus proches de la question
var vQuestion = await Embedding(question);
var scores = new List<(string Message, double Score)>();
foreach (var message in messagesChatEntreprise)
    scores.Add((message, Similarite(await Embedding(message), vQuestion)));

foreach (var (message, score) in scores.OrderByDescending(s => s.Score))
    Console.WriteLine($"{score:F2}  {message}");

var trouves = scores.OrderByDescending(s => s.Score).Take(2).Select(s => s.Message);

// 2. AUGMENTED : les ajouter au prompt
var prompt = $"""
    Réponds en une phrase, uniquement avec ces messages du chat d'entreprise :
    - {string.Join("\n- ", trouves)}

    Question : {question}
    """;

Console.WriteLine($"\nPrompt utilisé :\n{prompt}");

// 3. GENERATION : le LLM croise les infos et répond
Console.WriteLine($"\nSans RAG : {await Chat("Réponds en une phrase. " + question)}");
Console.WriteLine($"\nAvec RAG : {await Chat(prompt)}");

// --- Outils ---

// Texte -> vecteur de nombres qui représente son sens
async Task<float[]> Embedding(string texte)
{
    var rep = await http.PostAsJsonAsync("/api/embed", new { model = ModeleEmbedding, input = texte });
    return (await rep.Content.ReadFromJsonAsync<EmbedReponse>())!.Embeddings[0];
}

async Task<string> Chat(string message)
{
    var rep = await http.PostAsJsonAsync("/api/chat", new
    {
        model = ModeleChat,
        messages = new[] { new { role = "user", content = message } },
        stream = false,
        options = new { temperature = 0 }
    });
    return (await rep.Content.ReadFromJsonAsync<ChatReponse>())!.Message.Content.Trim();
}

// 1 = même sens, 0 = aucun rapport
static double Similarite(float[] a, float[] b)
{
    double ab = 0, aa = 0, bb = 0;
    for (var i = 0; i < a.Length; i++) { ab += a[i] * b[i]; aa += a[i] * a[i]; bb += b[i] * b[i]; }
    return ab / (Math.Sqrt(aa) * Math.Sqrt(bb));
}

record EmbedReponse(float[][] Embeddings);
record ChatReponse(ChatMessage Message);
record ChatMessage(string Content);
