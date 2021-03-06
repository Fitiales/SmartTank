﻿// 
//     Kerbal Engineer Redux
// 
//     Copyright (C) 2014 CYBUTEK
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Text;

namespace SmartTank.Simulation
{
	internal class AttachNodeSim
	{
		private static readonly Pool<AttachNodeSim> pool = new Pool<AttachNodeSim>(Create, Reset);

		public PartSim attachedPartSim;
		public String id;
		public AttachNode.NodeType nodeType;

		private static AttachNodeSim Create()
		{
			return new AttachNodeSim();
		}

		public static AttachNodeSim New(PartSim partSim, String newId, AttachNode.NodeType newNodeType)
		{
			AttachNodeSim nodeSim = pool.Borrow();

			nodeSim.attachedPartSim = partSim;
			nodeSim.nodeType = newNodeType;
			nodeSim.id = newId;

			return nodeSim;
		}

		static private void Reset(AttachNodeSim attachNodeSim)
		{
			attachNodeSim.attachedPartSim = null;
		}


		public void Release()
		{
			pool.Release(this);
		}

		public void DumpToLog(LogMsg log)
		{
			if (attachedPartSim == null)
			{
				log.Append("<staged>:<n>");
			}
			else
			{
				log.Append(attachedPartSim.name, ":", attachedPartSim.partId);
			}
			log.Append("#", nodeType, ":", id);
		}
	}
}