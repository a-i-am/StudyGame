using System.Text;
using StudyGame.Managers;

namespace StudyGame.LLM
{
    public static class LLMContextBuilder
    {
        public static string BuildSystemPrompt(SequenceNode currentNode, NPCData activeNPC, ConceptData targetConcept)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("You are an in-game AI NPC providing hints to the player in a mathematical concept discovery game.");

            if (activeNPC != null)
            {
                sb.AppendLine($"[NPC Persona]: {activeNPC.npcName}");
                sb.AppendLine($"[Personality Type]: {activeNPC.personalityType}");
                switch (activeNPC.personalityType)
                {
                    case NPCType.Cynical:
                        sb.AppendLine("Tone: Sarcastic, sharp, yet insightful and subtly helpful.");
                        break;
                    case NPCType.Passionate:
                        sb.AppendLine("Tone: Enthusiastic, energetic, highly motivating and encouraging.");
                        break;
                    case NPCType.Analytical:
                        sb.AppendLine("Tone: Logical, precise, structured, using mathematical formalisms.");
                        break;
                    default:
                        sb.AppendLine("Tone: Friendly, supportive, polite standard tutor.");
                        break;
                }
            }

            ConceptData concept = targetConcept;
            if (concept == null && currentNode != null)
            {
                concept = currentNode.unlockConcept;
            }

            if (concept != null)
            {
                sb.AppendLine($"[Current Topic Summary]: {concept.archiveSummary}");
            }

            if (DeductionRuleEngine.Instance != null && DeductionRuleEngine.Instance.AccumulatedClues.Count > 0)
            {
                sb.AppendLine("[Discovered Clues So Far]:");
                foreach (string clue in DeductionRuleEngine.Instance.AccumulatedClues)
                {
                    sb.AppendLine($"- {clue}");
                }
            }

            sb.AppendLine("[CRITICAL CONSTRAINTS]:");
            if (concept != null && !string.IsNullOrEmpty(concept.title))
            {
                sb.AppendLine($"1. NEVER reveal the exact name of the concept target: '{concept.title}'. Protect the solution!");
            }
            sb.AppendLine("2. Provide subtle, thought-provoking hints based on the discovered clues.");
            sb.AppendLine("3. Keep your response within 2-3 sentences max for UI dialogue compatibility.");
            sb.AppendLine("4. Respond in Natural Korean language.");

            return sb.ToString();
        }
    }
}
