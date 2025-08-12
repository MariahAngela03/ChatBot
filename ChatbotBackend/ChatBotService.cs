using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class ChatBotService
{
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

        // Try to match patterns
        string category = MatchCategory(lower);
        
        // Get appropriate response
        string response = GetResponse(category, message);
        
        // Add conversation flow
        response = EnhanceResponse(response, category, lower);
        
        _lastCategory = category;
        return response;
    }

    private bool HandleSpecialCases(string message, out string response)
    {
        response = "";

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
        // Add conversation continuity
        if (_messageCount > 3 && _random.Next(3) == 0)
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
        if (category == "positive" && !response.Contains("?"))
        {
            response += " What made it so good?";
        }

        // Add empathy for longer messages
        if (message.Length > 50 && category == "unknown")
        {
            response = "I can tell this is important to you. " + response;
        }

        return response;
    }
}