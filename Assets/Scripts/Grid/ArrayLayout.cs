using UnityEngine;

namespace Grid
{
	[System.Serializable]
	public class ArrayLayout  
	{ 
		[SerializeField]
		private RowData[] _rows = new RowData[14];

		public RowData[] Rows
		{
			get => _rows;
			set => _rows = value;
		} 
		
		[System.Serializable]
		public struct RowData
		{
			[SerializeField]
			private bool[] _column;

			public bool[] Column
			{
				get => _column; 
				set => _column = value;
			}
		}
	}
}
