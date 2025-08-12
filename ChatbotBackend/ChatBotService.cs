using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

public class ChatBotService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    
    private readonly Dictionary<string, List<string>> _responses = new()
    {
        ["greetings"] = new List<string>
        {
            "Hello! I'm your friendly bot. How can I help you today?",
            "Hi there! What's on your mind?",
            "Hey! Great to see you. What can I do for you?",
            "Hello! I'm here and ready to chat. What would you like to know?"
        },
        ["positive"] = new List<string>
        {
            "That's wonderful! I'm glad to hear that. What else is going well?",
            "Awesome! I love your positive energy. Tell me more!",
            "That's great! Thanks for sharing that with me.",
            "Nice! It's always good to hear positive things. What's next?"
        },
        ["questions"] = new List<string>
        {
            "That's a great question! Let me think about that...",
            "Interesting question! Here's what I think...",
            "Good point! I'd love to explore that with you.",
            "That's something worth discussing! My take is..."
        },
        ["help"] = new List<string>
        {
            "I can help with conversations, answer questions, or just chat! What interests you?",
            "I'm here to assist! I can discuss topics, help brainstorm, or simply be a friendly ear.",
            "There are lots of ways I can help - ask me anything or tell me what's on your mind!",
            "I love helping out! Whether you need info, want to chat, or have questions - I'm your bot!"
        },
        ["goodbye"] = new List<string>
        {
            "Goodbye! Thanks for the great chat!",
            "See you later! Have an amazing day!",
            "Take care! It was wonderful talking with you!",
            "Bye! Come back anytime you want to chat!"
        },
        ["unknown"] = new List<string>
        {
            "That's interesting! Tell me more about that.",
            "I'd love to learn more about what you're thinking.",
            "That sounds intriguing! Can you elaborate?",
            "I'm curious about your perspective on that. Share more!",
            "Hmm, that's a new one for me! Help me understand better."
        },
        ["emotional_support"] = new List<string>
        {
            "I'm really sorry you're going through this. That sounds incredibly tough.",
            "That must be really hard for you. I'm here if you want to talk about it.",
            "I can't imagine how difficult this must be. You don't have to go through this alone.",
            "That sounds really painful. Take your time - I'm here to listen.",
            "I'm so sorry. That kind of pain is real and it's okay to feel broken right now."
        },
        ["casual"] = new List<string>
        {
            "Yeah, totally get that vibe!",
            "For real! What's up with that?",
            "I feel you on that one.",
            "That's the mood right there.",
            "Same energy! Tell me more."
        },
        ["excited"] = new List<string>
        {
            "OMG yes! That's amazing!",
            "No way! That's so cool!",
            "YESSS! I'm so happy for you!",
            "That's incredible! Tell me everything!",
            "I'm literally so excited for you right now!"
        }
    };

    private readonly Dictionary<string, string[]> _patterns = new()
    {
        ["greetings"] = new[] { @"\b(hi|hello|hey|good morning|good afternoon|good evening|greetings)\b" },
        ["positive"] = new[] { 
            @"\b(nice|good|great|awesome|cool|amazing|wonderful|excellent|fantastic|perfect|love it|brilliant)\b",
            @"\b(thank you|thanks|appreciate|grateful)\b"
        },
        ["questions"] = new[] { 
            @"\b(what|how|why|when|where|who|can you|do you|are you|will you)\b.*\?",
            @"\b(explain|tell me about|help me understand)\b"
        },
        ["help"] = new[] { 
            @"\b(help|assist|support|what can you do|capabilities)\b"
        },
        ["goodbye"] = new[] { 
            @"\b(bye|goodbye|see you|farewell|take care|gotta go|talk later)\b"
        },
        ["feeling"] = new[] {
            @"\b(feel|feeling|emotion|mood|happy|sad|excited|worried|anxious|stressed)\b"
        },
        ["emotional_distress"] = new[] {
            @"\b(broken|heartbroken|devastated|crushed|destroyed|hurt|pain|suffering)\b",
            @"\b(left me|dumped|breakup|broke up|divorce|cheating|betrayed)\b",
            @"\b(depressed|suicidal|want to die|kill myself|end it all)\b"
        },
        ["casual_slang"] = new[] {
            @"\b(bro|dude|man|yo|sup|whatup|wazzup|lol|lmao|fr|ngl|tbh)\b"
        },
        ["excited"] = new[] {
            @"!{2,}|\b(amazing|incredible|awesome|fantastic|omg|wow|yes|yay)\b.*!",
            @"\b(so happy|thrilled|pumped|stoked|hyped)\b"
        },
        ["advice"] = new[] {
            @"\b(advice|help me|what should i do|recommend|suggest)\b"
        },
        ["joke"] = new[] {
            @"\b(joke|funny|laugh|humor|tell me something funny)\b"
        },
        ["quote"] = new[] {
            @"\b(quote|inspiration|motivate|wisdom|inspire me)\b"
        },
        ["fact"] = new[] {
            @"\b(fact|interesting|tell me about|random fact|did you know)\b"
        },
        ["time"] = new[] {
            @"\b(time|what time|clock|hour|minute|today|now)\b"
        }
    };

    private readonly Random _random = new Random();
    private int _messageCount = 0;
    private string _lastCategory = "";

    public string GetReply(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) 
            return "I'm here! What would you like to talk about?";

        _messageCount++;
        var lower = message.Trim().ToLowerInvariant();

        // Handle special cases first
        if (HandleSpecialCases(lower, out string specialResponse))
            return specialResponse;

        // Try API responses for dynamic content
        var apiResponse = TryGetApiResponse(lower).Result;
        if (!string.IsNullOrEmpty(apiResponse))
            return apiResponse;

        // Try to match patterns
        string category = MatchCategory(lower);
        
        // Get appropriate response
        string response = GetResponse(category, message);
        
        // Add conversation flow
        response = EnhanceResponse(response, category, lower);
        
        _lastCategory = category;
        return response;
    }

    private async Task<string> TryGetApiResponse(string message)
    {
        try
        {
            // Check for joke requests
            if (Regex.IsMatch(message, @"\b(joke|funny|laugh|humor|tell me something funny)\b"))
            {
                return await GetJoke();
            }

            // Check for inspirational quote requests
            if (Regex.IsMatch(message, @"\b(quote|inspiration|motivate|wisdom|inspire me|need motivation)\b"))
            {
                return await GetInspirationalQuote();
            }

            // Check for random fact requests
            if (Regex.IsMatch(message, @"\b(fact|interesting|tell me about|random fact|did you know|learn something)\b"))
            {
                return await GetRandomFact();
            }

            // Check for advice requests (get a random quote as advice)
            if (Regex.IsMatch(message, @"\b(advice|help me decide|what should i do|guidance)\b") && !Regex.IsMatch(message, @"\b(broken|hurt|sad|depressed)\b"))
            {
                return await GetAdviceQuote();
            }

            // Check for activity suggestions when bored
            if (Regex.IsMatch(message, @"\b(bored|nothing to do|activity|suggest something)\b"))
            {
                return await GetActivitySuggestion();
            }
        }
        catch (Exception)
        {
            // If API fails, return null to fall back to regular responses
            return null;
        }

        return null;
    }

    private async Task<string> GetJoke()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("https://official-joke-api.appspot.com/random_joke");
            var joke = JsonSerializer.Deserialize<JsonElement>(response);
            
            var setup = joke.GetProperty("setup").GetString();
            var punchline = joke.GetProperty("punchline").GetString();
            
            return $"{setup}\n\n{punchline} 😄";
        }
        catch
        {
            return "Here's a joke for you: Why don't scientists trust atoms? Because they make up everything! 😄";
        }
    }

    private async Task<string> GetInspirationalQuote()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("https://zenquotes.io/api/random");
            var quotes = JsonSerializer.Deserialize<JsonElement[]>(response);
            
            if (quotes.Length > 0)
            {
                var quote = quotes[0].GetProperty("q").GetString();
                var author = quotes[0].GetProperty("a").GetString();
                
                return $"✨ \"{quote}\" - {author}";
            }
        }
        catch
        {
            // Fallback inspirational quotes
            var fallbackQuotes = new[]
            {
                "✨ \"The only way to do great work is to love what you do.\" - Steve Jobs",
                "✨ \"Innovation distinguishes between a leader and a follower.\" - Steve Jobs",
                "✨ \"Life is what happens to you while you're busy making other plans.\" - John Lennon",
                "✨ \"The future belongs to those who believe in the beauty of their dreams.\" - Eleanor Roosevelt"
            };
            return fallbackQuotes[_random.Next(fallbackQuotes.Length)];
        }
        
        return null;
    }

    private async Task<string> GetRandomFact()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("https://uselessfacts.jsph.pl/random.json?language=en");
            var fact = JsonSerializer.Deserialize<JsonElement>(response);
            
            var factText = fact.GetProperty("text").GetString();
            return $"🤓 Did you know? {factText}";
        }
        catch
        {
            // Fallback facts
            var fallbackFacts = new[]
            {
                "🤓 Did you know? Honey never spoils! Archaeologists have found edible honey in ancient Egyptian tombs.",
                "🤓 Did you know? A group of flamingos is called a 'flamboyance'!",
                "🤓 Did you know? Bananas are berries, but strawberries aren't!",
                "🤓 Did you know? The shortest war in history lasted only 38-45 minutes!"
            };
            return fallbackFacts[_random.Next(fallbackFacts.Length)];
        }
    }

    private async Task<string> GetAdviceQuote()
    {
        try
        {
            // Use inspirational quotes for advice
            var quote = await GetInspirationalQuote();
            if (!string.IsNullOrEmpty(quote))
            {
                return $"Here's some wisdom that might help: {quote}";
            }
        }
        catch { }

        var adviceQuotes = new[]
        {
            "💡 \"Trust yourself. You know more than you think you do.\" - Benjamin Spock",
            "💡 \"The best time to plant a tree was 20 years ago. The second best time is now.\" - Chinese Proverb",
            "💡 \"Don't let yesterday take up too much of today.\" - Will Rogers",
            "💡 \"You miss 100% of the shots you don't take.\" - Wayne Gretzky"
        };
        return adviceQuotes[_random.Next(adviceQuotes.Length)];
    }

    private async Task<string> GetActivitySuggestion()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("https://www.boredapi.com/api/activity");
            var activity = JsonSerializer.Deserialize<JsonElement>(response);
            
            var activityText = activity.GetProperty("activity").GetString();
            var type = activity.GetProperty("type").GetString();
            
            return $"🎯 Here's an idea: {activityText}\n\nCategory: {char.ToUpper(type[0]) + type.Substring(1)}";
        }
        catch
        {
            var fallbackActivities = new[]
            {
                "🎯 Here's an idea: Go for a walk and take photos of interesting things you see!",
                "🎯 Here's an idea: Try cooking a new recipe you've never made before!",
                "🎯 Here's an idea: Write a short story or poem about your day!",
                "🎯 Here's an idea: Learn 5 new words in a language you're interested in!"
            };
            return fallbackActivities[_random.Next(fallbackActivities.Length)];
        }
    }

    private bool HandleSpecialCases(string message, out string response)
    {
        response = "";

        // Handle emotional distress - PRIORITY
        if (Regex.IsMatch(message, @"\b(broken|heartbroken|devastated|crushed|destroyed)\b"))
        {
            var supportResponses = new[]
            {
                "I'm so sorry you're feeling this way. That kind of pain is real and overwhelming.",
                "Being broken hurts so much. You don't have to carry this alone.",
                "I can't imagine how much pain you're in right now. I'm here with you.",
                "That sounds absolutely devastating. Your feelings are completely valid.",
                "I'm really sorry. When we're broken, everything feels impossible. But you're not alone."
            };
            response = supportResponses[_random.Next(supportResponses.Length)];
            return true;
        }

        // Handle relationship issues
        if (Regex.IsMatch(message, @"\b(gf left|bf left|girlfriend left|boyfriend left|dumped|broke up|left me)\b"))
        {
            var breakupResponses = new[]
            {
                "Oh no... breakups are absolutely brutal. I'm so sorry you're going through this.",
                "That's one of the worst feelings ever. Losing someone you care about is devastating.",
                "I'm really sorry. Being left by someone you love cuts deep. How are you holding up?",
                "Damn, that's rough. Heartbreak is no joke. Want to talk about what happened?",
                "I'm so sorry. That kind of rejection and loss hurts in ways people don't understand."
            };
            response = breakupResponses[_random.Next(breakupResponses.Length)];
            return true;
        }

        // Handle casual/slang
        if (Regex.IsMatch(message, @"\b(bro|dude|yo|sup|whatup|fr|ngl|tbh)\b"))
        {
            var casualResponses = new[]
            {
                "Yo! What's good?",
                "Hey dude! What's up?",
                "Sup! What's on your mind?",
                "Hey! What's the vibe today?",
                "Yo yo! Talk to me!"
            };
            response = casualResponses[_random.Next(casualResponses.Length)];
            return true;
        }

        // Handle time requests
        if (Regex.IsMatch(message, _patterns["time"][0]))
        {
            response = $"The current time is {DateTime.Now:HH:mm}. What else can I help you with?";
            return true;
        }

        // Handle feelings
        if (_patterns.ContainsKey("feeling") && Regex.IsMatch(message, _patterns["feeling"][0]))
        {
            var feelingResponses = new[]
            {
                "I appreciate you sharing how you're feeling. Emotions are important!",
                "Thank you for opening up about that. How has your day been overall?",
                "It's good that you're in touch with your feelings. Want to talk about it?",
                "I'm glad you feel comfortable sharing that with me. What's been on your mind?"
            };
            response = feelingResponses[_random.Next(feelingResponses.Length)];
            return true;
        }

        // Handle repeated interactions
        if (_messageCount > 1 && message == "hi" && _lastCategory == "greetings")
        {
            response = "Hi again! What's new with you?";
            return true;
        }

        return false;
    }

    private string MatchCategory(string message)
    {
        // Score each category
        var scores = new Dictionary<string, int>();
        
        foreach (var (category, patterns) in _patterns)
        {
            scores[category] = 0;
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(message, pattern, RegexOptions.IgnoreCase);
                scores[category] += matches.Count * 10; // Weight pattern matches highly
            }
        }

        // Return highest scoring category, or "unknown" if no matches
        var bestMatch = scores.OrderByDescending(x => x.Value).First();
        return bestMatch.Value > 0 ? bestMatch.Key : "unknown";
    }

    private string GetResponse(string category, string originalMessage)
    {
        if (!_responses.ContainsKey(category))
            category = "unknown";

        var responses = _responses[category];
        var baseResponse = responses[_random.Next(responses.Count)];

        // Personalize unknown responses
        if (category == "unknown")
        {
            // Try to incorporate part of their message
            var words = originalMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 2)
            {
                var keyWord = words.FirstOrDefault(w => w.Length > 4);
                if (!string.IsNullOrEmpty(keyWord))
                {
                    baseResponse = $"You mentioned '{keyWord}' - {baseResponse}";
                }
            }
        }

        return baseResponse;
    }

    private string EnhanceResponse(string response, string category, string message)
    {
        // Match user's communication style
        bool hasCasualVibes = Regex.IsMatch(message, @"\b(bro|dude|man|yo|sup|lol|fr|ngl)\b");
        bool hasExcitement = message.Contains("!") || Regex.IsMatch(message, @"\b(amazing|awesome|yes|yay)\b");
        bool isEmotional = Regex.IsMatch(message, @"\b(broken|hurt|sad|devastated|pain)\b");

        // Adjust tone based on user's style
        if (hasCasualVibes && category != "emotional_distress")
        {
            response = MakeCasual(response);
        }
        else if (hasExcitement && category != "emotional_distress")
        {
            response = AddExcitement(response);
        }
        else if (isEmotional)
        {
            response = MakeEmpathetic(response);
        }

        // Add conversation continuity (but not for emotional distress)
        if (_messageCount > 3 && _random.Next(3) == 0 && !isEmotional)
        {
            var continuationPhrases = new[]
            {
                " By the way, how has your day been going?",
                " What else is on your mind today?",
                " Is there anything specific you'd like to chat about?",
                " I'm enjoying our conversation!"
            };
            response += continuationPhrases[_random.Next(continuationPhrases.Length)];
        }

        // Add encouraging follow-ups for positive responses
        if (category == "positive" && !response.Contains("?") && !isEmotional)
        {
            response += " What made it so good?";
        }

        // Add empathy for longer messages or emotional content
        if ((message.Length > 50 || isEmotional) && category == "unknown")
        {
            response = "I can tell this is important to you. " + response;
        }

        return response;
    }

    private string MakeCasual(string response)
    {
        // Add casual flair without changing the core message
        var casualStarters = new[] { "Yo, ", "Dude, ", "Hey, ", "Bro, " };
        var casualEnders = new[] { " fr!", " ngl", " for real", "!" };
        
        if (_random.Next(3) == 0)
            response = casualStarters[_random.Next(casualStarters.Length)] + response.ToLower();
        
        if (_random.Next(3) == 0)
            response += casualEnders[_random.Next(casualEnders.Length)];
            
        return response;
    }

    private string AddExcitement(string response)
    {
        if (!response.EndsWith("!"))
            response += "!";
        
        var excitedStarters = new[] { "OMG ", "Wow ", "Yes! ", "That's amazing! " };
        if (_random.Next(2) == 0)
            response = excitedStarters[_random.Next(excitedStarters.Length)] + response;
            
        return response;
    }

    private string MakeEmpathetic(string response)
    {
        // Keep emotional responses gentle and supportive
        var empathyStarters = new[] { "I hear you. ", "I'm with you on this. ", "That's so valid. " };
        if (_random.Next(2) == 0 && !response.StartsWith("I"))
            response = empathyStarters[_random.Next(empathyStarters.Length)] + response;
            
        return response;
    }
}