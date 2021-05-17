using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arke.DSL.Step.Settings;

namespace Arke.DSL.Step
{
    public class Step
    {
        public NodeData NodeData { get; set; }
        public List<LinkData> LinkedSteps { get; set; }

        public int GetStepFromConnector(string connectorName)
        {
            try
            {
                return LinkedSteps.Single(s => s.FromPort == connectorName).To;
            }
            catch (Exception)
            {
                throw new StepNotFoundException($"Step at id: {NodeData.Key} is missing the Connector named: {connectorName}");
            }
        }
    }
}
