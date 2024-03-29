﻿/*
 *  This Source Code Form is subject to the terms of the Mozilla Public License,
 *  v. 2.0. If a copy of the MPL was not distributed with this file, You can
 *  obtain one at http://mozilla.org/MPL/2.0/.
 *
 *  The original code is copyright (c) 2022, open.mp team and contributors.
 */

namespace Upgrade
{
	class Diff
	{
		public int Line { get; set; }
		public string Description { get; set; }
		public string From { get; set; }
		public string To { get; set; }
	}
}

