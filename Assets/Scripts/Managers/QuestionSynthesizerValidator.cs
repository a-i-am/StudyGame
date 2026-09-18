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
        public SynthesisResult ValidateSynthesis(ValidationRuleSO rule, List<SentenceItemData> submittedItems)
        {
            SynthesisResult result = new SynthesisResult();

            if (rule == null || submittedItems == null || submittedItems.Count == 0)
            {
                result.isValid = false;
                result.feedbackMessage = "올바르지 않은 조립 요청입니다.";
                return result;
            }

            if (!rule.Validate(submittedItems))
            {
                result.isValid = false;
                result.feedbackMessage = "추론 논리가 규칙에 부합하지 않습니다.";
                return result;
            }

            // Climax Puzzle 'NaN Error' Logic
            bool hasOperator = false;
            foreach (var item in submittedItems)
            {
                if (item.category == SentenceCategory.Operator || item.category == SentenceCategory.Contradiction)
                {
                    hasOperator = true;
                    if (item.associatedSkill == null)
                    {
                        // NaN Error Logic: A rule was structurally correct but practically unsolvable
                        result.isValid = false;
                        result.feedbackMessage = "수식 오류(NaN): 대상을 타격할 유효한 스킬 연산자가 없습니다.";
                        return result;
                    }
                    result.resolvedSkill = item.associatedSkill;
                }
                
                if (item.associatedConcept != null)
                {
                    result.targetConcept = item.associatedConcept;
                }

                result.totalApCost += item.apCost;
            }

            if (!hasOperator)
            {
                result.isValid = false;
                result.feedbackMessage = "수식 오류(NaN): 연산자가 누락되었습니다.";
                return result;
            }

            result.isValid = true;
            result.feedbackMessage = "질문 조립 성공! 클라이맥스 논리 공격 준비 완료.";
            return result;
        }
    }
}
