using System;
using System.Collections.Generic;
using System.Linq;
using ECommons;
using ECommons.ImGuiMethods;

namespace GambaGames.Games;

public class Card
{
    public enum Suites
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }
    
    public int Value { get; set; }
    
    public Suites Suite { get; set; }
    
    public Card(int value, Suites suite)
    {
        Value = value;
        Suite = suite;
    }

    public override string ToString()
    {
        string suite = "";
        switch (Suite)
        {
            case Suites.Clubs:
                suite = "\u2663";
                break;
            case Suites.Diamonds:
                suite = "\u2666";
                break;
            case Suites.Hearts:
                suite = "\u2665";
                break;
            case Suites.Spades:
                suite = "\u2660";
                break;
        }

        if (Value is 11 or 1)
        {
            return $"A{suite}";
        }
        if (Value is 12)
        {
            return $"J{suite}";
        }
        if (Value is 13)
        {
            return $"Q{suite}";
        }
        if (Value is 14)
        {
            return $"K{suite}";
        }

        return $"{Value}{suite}";
    }
}

public static class Deck
{
    private static List<Card> CardDeck = new List<Card>();
        
    public static void CreateDeck(int numDecks)
    {
        for (int k = 0; k < numDecks; k++)
        {
            for (int i = 0; i < 52; i++)
            {
                Card.Suites suite = (Card.Suites)(Math.Floor((decimal)i / 13));
                int val = i % 13 + 1;
                CardDeck.Add(new Card(val, suite));
            }
        }
    }
    
    public static List<Card> GetDeck()
    {
        return CardDeck;
    }

    public static void ClearDeck()
    {
        CardDeck.Clear();
    }

    // Fisher-Yates shuffle
    public static void Shuffle()
    {
        Notify.Info("Shuffling");
        Random rng = new Random();
        int count = CardDeck.Count-1;
        for (int i = count; i > 1; i--) 
        {  
            int k = rng.Next(i + 1);
            (CardDeck[k], CardDeck[i]) = (CardDeck[i], CardDeck[k]);
        } 
    }
}

public static class Hands
{
    private static List<KeyValuePair<string, List<string>>> PlayerHands = new();
    private static List<string> HasHit = new List<string>();
    private static List<string> HasSplit = new List<string>();
    private static List<string> SplitAces = new();
    private static List<string> GameOver = new();
    
    public static void Draw(string player, int decks, bool initialHand)
    {
        if (Deck.GetDeck().Count == 0)
        {
            Notify.Error("Ran out of cards, created new deck!");
            Deck.CreateDeck(decks);
        }
        
        if (PlayerHands.FirstOrDefault(x => x.Key == player).Key == null)
        {
            PlayerHands.Add(new KeyValuePair<string, List<string>>(player, new List<string>()));
        }
        PlayerHands.First(x => x.Key == player).Value.Add(Deck.GetDeck()[0].ToString());
        Deck.GetDeck().Remove(Deck.GetDeck()[0]);

        if (SplitAces.Contains(player)) GameOver.Add(player);
        
        if(!HasHit.Contains(player) && !initialHand) HasHit.Add(player);
    }

    public static string GetHand(string player, bool censorCards)
    {
        if (PlayerHands.FirstOrDefault(x => x.Key == player).Key == null)
        {
            PlayerHands.Add(new KeyValuePair<string, List<string>>(player, new List<string>()));
        }
        string cardsInHand = "";
        List<string> hand = PlayerHands.First(x => x.Key == player).Value;

        int counter = 0;
        foreach (var card in hand)
        {
            if (censorCards && hand.Count == 2)
            {
                if (counter == 1)
                {
                    cardsInHand += $"?? ";
                }
                else
                {
                    cardsInHand += $"{card} ";
                }
            }
            else
            {
                cardsInHand += $"{card} ";
            }

            counter++;
        }
        return cardsInHand;
    }

    public static int HandValue(string hand, string player)
    {
        if (hand.IsNullOrEmpty()) return 0;
        
        int val = 0;
        int aces = 0;
        string[] cards = hand.Split(" ");
        foreach (string card in cards)
        {
            if (card.IsNullOrEmpty()) continue;
            string cardvalue;
            cardvalue = card.Replace("\u2663", "")
                            .Replace("\u2666", "")
                            .Replace("\u2665", "")
                            .Replace("\u2660", "");
            
            if (cardvalue == "A")
            {
                aces++;
                continue;
            }
            
            if (cardvalue is "J" or "Q" or "K")
            {
                val += 10;
                continue;
            }
            
            val += short.Parse(cardvalue);
        }

        if (aces != 0)
        {
            for (; aces > 0; aces--)
            {
                if (val > 10)
                {
                    val += 1;
                }
                else if (val == 10 && aces == 1)
                {
                    val += 11;
                }
                else if (val == 10 && aces > 1)
                {
                    val += 1;
                }
                else
                {
                    val += 11;
                }
            }
        }

        if (val >= 21)
        {
            if (!GameOver.Contains(player))
            {
                GameOver.Add(player);
            }
        }

        return val;
    }

    public static void Split(string player)
    {
        List<string> hand = PlayerHands.First(x => x.Key == player).Value;
        
        string newPlayer = $"{player} 2";
        List<string> newHand = new List<string>();
        newHand.Add(hand[1]);
        
        PlayerHands.Add(new KeyValuePair<string, List<string>>(newPlayer, newHand));
        PlayerHands.First(x => x.Key == player).Value.Remove(hand[1]);
        Draw(player, 2, true);
        Draw(newPlayer, 2, true);
        
        HasSplit.Add(player);
        HasSplit.Add($"{player} 2");
        
        if (hand[1].Contains("A"))
        {
            SplitAces.Add(player);
            SplitAces.Add($"{player} 2");
        }
    }

    public static bool DealerStay(string hand)
    {
        if (hand.IsNullOrEmpty()) return false;
        
        int val = 0;
        int aces = 0;
        bool softSeventeen = false;
        string[] cards = hand.Split(" ");
        
        foreach (string card in cards)
        {
            if (card.IsNullOrEmpty()) continue;
            string cardvalue;
            cardvalue = card.Replace("\u2663", "")
                            .Replace("\u2666", "")
                            .Replace("\u2665", "")
                            .Replace("\u2660", "");
            
            if (cardvalue == "A")
            {
                aces++;
                continue;
            }
            if (cardvalue is "J" or "Q" or "K")
            {
                val += 10;
                continue;
            }
            val += short.Parse(cardvalue);
        }

        if (aces != 0)
        {
            for (int i = 0; i < aces; i++)
            {
                if (val > 10)
                {
                    val += 1;
                }
                else if (val == 10 && aces == 1)
                {
                    val += 11;
                    softSeventeen = true;
                }
                else if (val == 10 && aces > 1)
                {
                    val += 1;
                }
                else
                {
                    val += 11;
                    softSeventeen = true;
                }
            }
        }

        if (val == 21) return true;

        if (val >= 18) return true;

        if (softSeventeen) return false;
        
        return val >= 17;
    }
    
    public static bool CanSplit(string hand, string player)
    {
        if (hand.IsNullOrEmpty()) return false;

        if (HasSplit.Contains(player.Replace(" 2", ""))) return false;
        
        string[] cards = hand.Substring(0,hand.LastIndexOf(" ")).Split(" ");

        int counter = 0;
        
        for (var i = 0; i < cards.Length; i++)
        {
            cards[i] = cards[i].Replace("\u2663", "")
                               .Replace("\u2666", "")
                               .Replace("\u2665", "")
                               .Replace("\u2660", "");
            
            if (cards[i] is "J" or "Q" or "K")
            {
                cards[i] = "10";
            }
        }
        
        if (cards.Length == 2)
        {
            if (cards[0] == cards[1])
            {
                return true;
            }
        }
        
        return false;
    }

    public static void DoubleDown(string player, int decks)
    {
        if (Deck.GetDeck().Count == 0)
        {
            Notify.Error("Ran out of cards, created new deck!");
            Deck.CreateDeck(decks);
        }
        
        PlayerHands.First(x => x.Key == player).Value.Add(Deck.GetDeck()[0].ToString());
        Deck.GetDeck().Remove(Deck.GetDeck()[0]);
        GameOver.Add(player);
    }

    public static void ClearHands()
    {
        PlayerHands = new();
        HasHit.Clear();
        HasSplit.Clear();
        SplitAces.Clear();
        GameOver.Clear();
    }
    
    public static bool IsHandNatural(string player)
    {
        return !HasHit.Contains(player);
    }

    public static bool IsPlayerGameOver(string player)
    {
        return GameOver.Contains(player);
    }
    
    public static void SetGameOver(string player)
    {
        GameOver.Add(player);
    }
    
    public static bool IsGameOver(List<string> players)
    {
        foreach (var player in players)
        {
            if (!GameOver.Contains(player)) return false;
        }
        return true;
    }
}
