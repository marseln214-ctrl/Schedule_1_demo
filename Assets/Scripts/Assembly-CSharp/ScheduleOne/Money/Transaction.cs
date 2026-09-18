namespace ScheduleOne.Money
{
	public class Transaction
	{
		public string transaction_Name = string.Empty;

		public float unit_Amount;

		public float quantity = 1f;

		public string transaction_Note = string.Empty;

		public float total_Amount => unit_Amount * quantity;

		public Transaction(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
		{
			transaction_Name = _transaction_Name;
			unit_Amount = _unit_Amount;
			quantity = _quantity;
			transaction_Note = _transaction_Note;
		}
	}
}
