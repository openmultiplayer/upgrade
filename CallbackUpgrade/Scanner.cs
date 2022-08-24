using System;
using System.Collections.Generic;
using System.Text;

namespace CallbackUpgrade
{
	class Scanner
	{
		public bool Report { private set; get; }

		public Scanner(bool report)
		{
			Report = report;
		}
	}
}

