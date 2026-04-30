using System.Collections;
using Project.Runtime.Scripts.AI;
using UnityEngine;

namespace Project.Runtime.Scripts.Systems
{
    public class GameDialogueSystem : MonoBehaviour
    {
        private const float FILLER_MIN_DELAY = 10f;
        private const float FILLER_MAX_DELAY = 20f;
        private const float THINKING_MIN_DELAY = 8f;
        private const float THINKING_MAX_DELAY = 15f;
        private const string PLAYER_KEY = "Player";

        private static readonly string[] JUDGING_MESSAGES = 
        {
            "Pencils down! Time for me to judge these... masterpieces.",
            "Alright, drawing phase is over. Time for my expert verdict.",
            "Stop drawing! Let me see what on earth you two just did.",
            "Time is up! Step aside and let the master judge do his job.",
            "Hands off the canvas! Let's see who butchered this word the least.",
            "That's a wrap! Prepare yourselves for my brutally honest critique.",
            "Ding, ding, ding! Time's up. Let's examine the damage.",
            "Drop the pens! It's time for the moment of truth.",
            "Time's up! Let's find out which one of you actually knows what a shape is.",
            "And stop! Let the supreme judge determine who gets the point."
        };

        private static readonly string[] THINKING_MESSAGES = 
        {
            "Hmm...",
            "I see, I see...",
            "My god...",
            "Well... this is certainly something.",
            "Interesting approach...",
            "Oh boy...",
            "Wait, what exactly is that?",
            "Let me put my glasses on for this one.",
            "Yikes...",
            "Uh huh...",
            "Wow... just, wow.",
            "Okay, let me process this.",
            "Is this supposed to be modern art?",
            "Fascinating... and deeply confusing.",
            "Alright, let's take a closer look.",
            "Oh, sweet merciful heavens...",
            "Hmm... intriguing.",
            "Good grief.",
            "Let me think about this...",
            "Oof...",
            "Alright...",
            "Okay...",
            "Sure...",
            "Right...",
            "Let's see...",
            "I'm looking...",
            "Give me a second.",
            "Just a moment.",
            "Let me check.",
            "What do we have here?",
            "This is... different.",
            "Aha.",
            "Mhm.",
            "Yeah...",
            "I guess...",
            "I mean...",
            "Well then.",
            "Okay then.",
            "Let me see.",
            "Let's find out.",
            "Hold on.",
            "Let me examine this.",
            "Taking a look.",
            "Oh.",
            "Oh my.",
            "Dear me.",
            "Let me process.",
            "Thinking...",
            "Looking closely.",
            "Very well.",
            "Not bad...",
            "I see.",
            "Got it.",
            "Understood.",
            "Let's review.",
            "Reviewing...",
            "Analyzing...",
            "Wait.",
            "Interesting.",
            "Oh dear."
        };

        private static readonly string[] PLAYER_DONE_MESSAGES =
        {
            "The human is done. Let's see if speed equals quality.",
            "Oh, finished already? Let's hope it's not a complete disaster.",
            "Pencils down for the human! Let's see if the AI can do better.",
            "Fast work! But is it good work? We shall see."
        };

        private static readonly string[] FILLER_MESSAGES =
        {
            "Take your time, but not too much time.",
            "I am judging you silently right now.",
            "Is that what I think it is? Oh dear.",
            "I have seen better drawings from a toaster.",
            "Fascinating technique... if you can call it that.",
            "Don't worry, Picasso was misunderstood in his time too.",
            "This is truly a test of my patience.",
            "Are you drawing with your eyes closed?",
            "I'm starting to regret my life choices watching this."
        };

        private static readonly string[] PLAYER_WIN_MESSAGES =
        {
            "The human actually won! This is another episode of Sketchy Business, hope you liked it, and see you next time!",
            "Against all odds, the human takes the crown! That was another episode of Sketchy Business, hope you enjoyed it, and see you next time!"
        };

        private static readonly string[] AI_WIN_MESSAGES =
        {
            "The AI reigns supreme! And that was another episode of Sketchy Business, hope you enjoyed it, and see you next time!",
            "Superior machine intellect wins again! Thank you for joining us for another episode of Sketchy Business. Hope you liked it, and see you next time!"
        };

        [Header("References")]
        [SerializeField] private TtsSystem _ttsSystem;
        [SerializeField] private AiDialogueSystem _aiDialogueSystem;
        
        [Header("Timing")]
        [SerializeField] private float _introSpeechDelay = 2.5f;
        
        private Coroutine _fillerRoutine;
        private Coroutine _thinkingRoutine;
        private bool _canPlayFiller;
        private bool _isThinking;

        public void PlayIntro()
        {
            if (_aiDialogueSystem == null) return;
            _aiDialogueSystem.RequestIntroDialogue(HandleIntroDialogueReceived);
        }

        public void PlayTopicAnnouncement(int round, string category, string word)
        {
            if (_ttsSystem == null) return;
            var announcement = $"Round {round}. The category is {category}, and your word is {word}. Good luck!";
            _ttsSystem.Speak(announcement, true);
        }

        public void PlayPlayerDone()
        {
            if (_ttsSystem == null) return;
            var msg = PLAYER_DONE_MESSAGES[Random.Range(0, PLAYER_DONE_MESSAGES.Length)];
            _ttsSystem.Speak(msg, true);
        }

        public void PlayGameOver(string winner)
        {
            if (_ttsSystem == null) return;
            var messages = winner == PLAYER_KEY ? PLAYER_WIN_MESSAGES : AI_WIN_MESSAGES;
            var msg = messages[Random.Range(0, messages.Length)];
            _ttsSystem.Speak(msg, true);
        }

        public void PlayJudgingIntro()
        {
            if (_ttsSystem == null) return;
            var judgingMsg = JUDGING_MESSAGES[Random.Range(0, JUDGING_MESSAGES.Length)];
            _ttsSystem.Speak(judgingMsg, true);
        }

        public void StartThinkingFiller()
        {
            StopThinkingFiller();
            _isThinking = true;
            _thinkingRoutine = StartCoroutine(ThinkingFillerRoutineAsync());
        }

        public void StopThinkingFiller()
        {
            _isThinking = false;
            if (_thinkingRoutine != null)
            {
                StopCoroutine(_thinkingRoutine);
                _thinkingRoutine = null;
            }
        }

        public void PlayJudgeFeedback(string feedback)
        {
            if (_ttsSystem == null) return;
            _ttsSystem.Speak(feedback, true);
        }

        public void StartFillerRoutine()
        {
            StopFillerRoutine();
            _canPlayFiller = true;
            _fillerRoutine = StartCoroutine(FillerDialogueRoutineAsync());
        }

        public void StopFillerRoutine()
        {
            _canPlayFiller = false;
            if (_fillerRoutine != null)
            {
                StopCoroutine(_fillerRoutine);
                _fillerRoutine = null;
            }
        }

        public void CancelAllDialogues()
        {
            if (_aiDialogueSystem != null) _aiDialogueSystem.CancelAllRequests();
            StopFillerRoutine();
            StopThinkingFiller();
        }

        private IEnumerator FillerDialogueRoutineAsync()
        {
            while (_canPlayFiller)
            {
                yield return new WaitForSeconds(Random.Range(FILLER_MIN_DELAY, FILLER_MAX_DELAY));
                if (!_canPlayFiller) yield break;

                if (_ttsSystem != null)
                {
                    var msg = FILLER_MESSAGES[Random.Range(0, FILLER_MESSAGES.Length)];
                    _ttsSystem.Speak(msg, false);
                }
            }
        }

        private IEnumerator ThinkingFillerRoutineAsync()
        {
            while (_isThinking)
            {
                yield return new WaitForSeconds(Random.Range(THINKING_MIN_DELAY, THINKING_MAX_DELAY));
                if (!_isThinking) yield break;

                if (_ttsSystem != null)
                {
                    var msg = THINKING_MESSAGES[Random.Range(0, THINKING_MESSAGES.Length)];
                    _ttsSystem.Speak(msg, false);
                }
            }
        }

        private void HandleIntroDialogueReceived(string text)
        {
            var speech = string.IsNullOrEmpty(text) ? "Welcome to Sketchy Business! I'm your host, and the rules are simple: first to three points wins. Let's get drawing!" : text;
            StartCoroutine(SpeakIntroWithDelayAsync(speech));
        }
        
        private IEnumerator SpeakIntroWithDelayAsync(string text)
        {
            yield return new WaitForSeconds(_introSpeechDelay);
            
            if (_ttsSystem != null) 
                _ttsSystem.Speak(text, true);
        }
    }
}