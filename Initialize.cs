using System;
using UnityEngine;

namespace TinyMUD
{
	public static class Initialize
	{
		public static void Load()
		{
			Clock.Initialize();
			Loop.Initialize();
		}
	}
}