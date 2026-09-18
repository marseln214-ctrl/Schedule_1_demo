using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone
{
	public class CustomerSelector : MonoBehaviour
	{
		public GameObject ButtonPrefab;

		[Header("References")]
		public RectTransform EntriesContainer;

		public UnityEvent<Customer> onCustomerSelected;

		private List<RectTransform> customerEntries = new List<RectTransform>();

		private Dictionary<RectTransform, Customer> entryToCustomer = new Dictionary<RectTransform, Customer>();

		public void Awake()
		{
			for (int i = 0; i < Customer.UnlockedCustomers.Count; i++)
			{
				CreateEntry(Customer.UnlockedCustomers[i]);
			}
			Customer.onCustomerUnlocked = (Action<Customer>)Delegate.Combine(Customer.onCustomerUnlocked, new Action<Customer>(CreateEntry));
			Close();
		}

		public void Start()
		{
			GameInput.RegisterExitListener(Exit, 7);
		}

		private void OnDestroy()
		{
			Customer.onCustomerUnlocked = (Action<Customer>)Delegate.Remove(Customer.onCustomerUnlocked, new Action<Customer>(CreateEntry));
		}

		private void Exit(ExitAction action)
		{
			if (action != null && !action.used && this != null && base.gameObject != null && base.gameObject.activeInHierarchy)
			{
				action.used = true;
				Close();
			}
		}

		public void Open()
		{
			for (int i = 0; i < customerEntries.Count; i++)
			{
				if (entryToCustomer[customerEntries[i]].AssignedDealer != null)
				{
					customerEntries[i].gameObject.SetActive(value: false);
				}
				else
				{
					customerEntries[i].gameObject.SetActive(value: true);
				}
			}
			base.gameObject.SetActive(value: true);
		}

		public void Close()
		{
			base.gameObject.SetActive(value: false);
		}

		private void CreateEntry(Customer customer)
		{
			RectTransform component = UnityEngine.Object.Instantiate(ButtonPrefab, EntriesContainer).GetComponent<RectTransform>();
			component.Find("Mugshot").GetComponent<Image>().sprite = customer.NPC.MugshotSprite;
			component.Find("Name").GetComponent<Text>().text = customer.NPC.fullName;
			component.GetComponent<Button>().onClick.AddListener(delegate
			{
				CustomerSelected(customer);
			});
			customerEntries.Add(component);
			entryToCustomer.Add(component, customer);
		}

		private void CustomerSelected(Customer customer)
		{
			if (customer.AssignedDealer == null && onCustomerSelected != null)
			{
				onCustomerSelected.Invoke(customer);
			}
			Close();
		}
	}
}
