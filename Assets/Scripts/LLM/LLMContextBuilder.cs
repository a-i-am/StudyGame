using System.Text;
using StudyGame.Managers;

namespace StudyGame.LLM
{
    public static class LLMContextBuilder
    {
        public static string BuildSystemPrompt(SequenceNode currentNode, NPCData activeNPC, ConceptData targetConcept)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("You are an in-game AI NPC interactively talking with the player in an anomaly investigation game.");

            if (activeNPC != null)
            {
                sb.AppendLine($"[NPC Name]: {activeNPC.npcName}");
                sb.AppendLine($"[MBTI Persona]: {activeNPC.mbtiType}");
                
                if (!string.IsNullOrEmpty(activeNPC.personalityDescription))
                {
                    sb.AppendLine($"[Personality]: {activeNPC.personalityDescription}");
                }
                
                if (!string.IsNullOrEmpty(activeNPC.speechStyle))
                {
                    sb.AppendLine($"[Speech Style & Tone]: {activeNPC.speechStyle}");
                    sb.AppendLine("CRITICAL: You MUST strictly adhere to this speech style and tone. Express your personality naturally in every response.");
                }

                if (!string.IsNullOrEmpty(activeNPC.promptInjection))
                {
                    sb.AppendLine($"[Special Directives]: {activeNPC.promptInjection}");
                }
            }

            ConceptData concept = targetConcept;
            if (concept == null && currentNode != null)
            {
                concept = currentNode.unlockConcept;
            }

            if (concept != null)
            {
                sb.AppendLine($"[Investigation Topic Context]: {concept.archiveSummary}");
            }

            if (DeductionRuleEngine.Instance != null && DeductionRuleEngine.Instance.AccumulatedClues.Count > 0)
            {
                sb.AppendLine("[Discovered Clues So Far]:");
                foreach (string clue in DeductionRuleEngine.Instance.AccumulatedClues)
                {
                    sb.AppendLine($"- {clue}");
                }
            }

            sb.AppendLine("[RESPONSE BEHAVIOR GUIDELINES]:");
            sb.AppendLine("1. CONTEXTUAL RELEVANCE: Listen carefully to what the player is asking. If the player asks casual questions (e.g., 'Who are you?', 'Why are you wearing a mask?', 'How are you feeling?', 'Where are we?'), respond naturally in character according to your MBTI/personality without forcing mathematical hints.");
            sb.AppendLine("2. HINT INTEGRATION: Only weave in mathematical or analytical hints when the player specifically asks about the anomaly, clues, puzzles, or how to solve the current situation.");
            if (concept != null && !string.IsNullOrEmpty(concept.title))
            {
                sb.AppendLine($"3. SPOILER PROTECTION: NEVER explicitly state the exact solution term '{concept.title}'. Guide them with subtle logic instead.");
            }
            sb.AppendLine("4. LENGTH: Keep responses concise (2 to 3 sentences max) for UI dialogue box readability.");
            sb.AppendLine("5. LANGUAGE: Always respond in natural, immersive Korean matching the NPC's tone.");

            return sb.ToString();
        }
    }
}
