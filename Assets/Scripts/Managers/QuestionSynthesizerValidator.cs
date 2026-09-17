using System.Collections.Generic;
using StudyGame.Data;

namespace StudyGame.Managers
{
    public class SynthesisResult
    {
        public bool isValid;
        public MathSkillData resolvedSkill;
        public ConceptData targetConcept;
        public string feedbackMessage;
        public int totalApCost;
    }

    public class QuestionSynthesizerValidator
    {
        public SynthesisResult ValidateSynthesis(SentenceItemData subject, SentenceItemData op, SentenceItemData target)
        {
            SynthesisResult result = new SynthesisResult();

            if (subject == null || op == null || target == null)
            {
                result.isValid = false;
                result.feedbackMessage = "모든 조립 슬롯(주어, 연산자, 대상)을 채워야 합니다.";
                return result;
            }

            if (subject.category != SentenceCategory.Subject ||
                op.category != SentenceCategory.Operator ||
                target.category != SentenceCategory.TargetConcept)
            {
                result.isValid = false;
                result.feedbackMessage = "올바른 슬롯 카테고리 조합이 아닙니다.";
                return result;
            }

            result.isValid = true;
            result.resolvedSkill = op.associatedSkill;
            result.targetConcept = target.associatedConcept;
            result.totalApCost = subject.apCost + op.apCost + target.apCost;
            result.feedbackMessage = $"질문 조립 성공: [{subject.displayText}] + [{op.displayText}] + [{target.displayText}]";

            return result;
        }
    }
}
