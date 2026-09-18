using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.Money;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.Quests;
using ScheduleOne.UI.Relations;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI
{
	public class DealCompletionPopup : Singleton<DealCompletionPopup>
	{
		[Header("References")]
		public Canvas Canvas;

		public RectTransform Container;

		public CanvasGroup Group;

		public Animation Anim;

		public TextMeshProUGUI Title;

		public TextMeshProUGUI PaymentLabel;

		public TextMeshProUGUI SatisfactionValueLabel;

		public RelationCircle RelationCircle;

		public TextMeshProUGUI RelationshipLabel;

		public Gradient SatisfactionGradient;

		public AudioSourceController SoundEffect;

		public TextMeshProUGUI[] BonusLabels;

		private Coroutine routine;

		public bool IsPlaying { get; protected set; }

		protected override void Awake()
		{
			base.Awake();
			Canvas.enabled = false;
			Container.gameObject.SetActive(value: false);
		}

		public void PlayPopup(Customer customer, float satisfaction, float originalRelationshipDelta, float basePayment, List<Contract.BonusPayment> bonuses)
		{
			IsPlaying = true;
			if (routine != null)
			{
				StopCoroutine(routine);
			}
			routine = StartCoroutine(Routine());
			IEnumerator Routine()
			{
				Group.alpha = 0f;
				Canvas.enabled = true;
				Container.gameObject.SetActive(value: true);
				Title.text = "Deal completed for " + customer.NPC.fullName;
				PaymentLabel.text = "+$0";
				SatisfactionValueLabel.text = "0%";
				SatisfactionValueLabel.color = SatisfactionGradient.Evaluate(0f);
				for (int l = 0; l < BonusLabels.Length; l++)
				{
					if (bonuses.Count > l)
					{
						BonusLabels[l].text = "<color=#54E717>+" + MoneyManager.FormatAmount(bonuses[l].Amount) + "</color> " + bonuses[l].Title;
						BonusLabels[l].gameObject.SetActive(value: true);
					}
					else
					{
						BonusLabels[l].gameObject.SetActive(value: false);
					}
				}
				yield return new WaitForSeconds(0.2f);
				Anim.Play();
				SoundEffect.Play();
				RelationCircle.AssignNPC(customer.NPC);
				RelationCircle.SetUnlocked(NPCRelationData.EUnlockType.Recommendation, notify: false);
				RelationCircle.SetNotchPosition(originalRelationshipDelta);
				SetRelationshipLabel(originalRelationshipDelta);
				yield return new WaitForSeconds(0.2f);
				float paymentLerpTime = 1.5f;
				for (float j = 0f; j < paymentLerpTime; j += Time.deltaTime)
				{
					PaymentLabel.text = "+" + MoneyManager.FormatAmount(basePayment * (j / paymentLerpTime));
					yield return new WaitForEndOfFrame();
				}
				PaymentLabel.text = "+" + MoneyManager.FormatAmount(basePayment);
				yield return new WaitForSeconds(1.5f);
				float satisfactionLerpTime = 1f;
				for (float j = 0f; j < satisfactionLerpTime; j += Time.deltaTime)
				{
					SatisfactionValueLabel.color = SatisfactionGradient.Evaluate(j / satisfactionLerpTime * satisfaction);
					SatisfactionValueLabel.text = Mathf.Lerp(0f, satisfaction, j / satisfactionLerpTime).ToString("P0");
					yield return new WaitForEndOfFrame();
				}
				SatisfactionValueLabel.color = SatisfactionGradient.Evaluate(satisfaction);
				SatisfactionValueLabel.text = satisfaction.ToString("P0");
				yield return new WaitForSeconds(0.25f);
				float endDelta = customer.NPC.RelationData.RelationDelta;
				float lerpTime = Mathf.Abs(customer.NPC.RelationData.RelationDelta - originalRelationshipDelta);
				for (float j = 0f; j < lerpTime; j += Time.deltaTime)
				{
					float num = Mathf.Lerp(originalRelationshipDelta, endDelta, j / lerpTime);
					RelationCircle.SetNotchPosition(num);
					SetRelationshipLabel(num);
					yield return new WaitForEndOfFrame();
				}
				RelationCircle.SetNotchPosition(endDelta);
				SetRelationshipLabel(endDelta);
				yield return new WaitUntil(() => Group.alpha == 0f);
				Canvas.enabled = false;
				Container.gameObject.SetActive(value: false);
				routine = null;
				IsPlaying = false;
			}
		}

		private void SetRelationshipLabel(float delta)
		{
			ERelationshipCategory category = RelationshipCategory.GetCategory(delta);
			RelationshipLabel.text = category.ToString();
			RelationshipLabel.color = RelationshipCategory.GetColor(category);
		}
	}
}
