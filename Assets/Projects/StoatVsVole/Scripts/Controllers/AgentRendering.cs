using UnityEngine;

namespace StoatVsVole
{
    public static class AgentRenderer
    {
        public static void UpdateLabel(AgentController agent)
        {
            if (agent.floatingLabel == null || agent.manager == null)
                return;

            if (!agent.manager.globalSettings.labelList.Contains(agent.agentTag))
                return;

            string labelText = $"{agent.agentTag}\nEnergy: {agent.energy:F1}";

            if (agent.withEnergySource)
                labelText += "\nExtracting";

            if (agent.withEnergySink)
                labelText += "\nBeing Drained";

            labelText += $"\nReward: {agent.GetCumulativeReward():F2}";

            agent.floatingLabel.SetLabel(
                labelText,
                Color.white,
                4.0f,
                new Vector3(0, agent.agentDefinition.bodySettings.scaleY + 0.25f, 0)
            );

            agent.lastLabelText = labelText;
        }

        public static void UpdateVisualState(AgentController agent)
        {



            var renderer = agent.bodyRenderer;
            if (renderer == null) return;

            float ratio = Mathf.Clamp01(agent.energy / agent.maxEnergy);
            Color energyColor;

            if (agent.CompareTag("vole"))
            {
                energyColor = ratio < 0.5f
                    ? Color.Lerp(Color.red, Color.green, ratio * 2f)
                    : Color.Lerp(Color.green, Color.yellow, (ratio - 0.5f) * 2f);
            }
            else if (agent.CompareTag("flower"))
            {
                energyColor = Color.Lerp(Color.red, Color.white, ratio);
            }
            else
            {
                energyColor = Color.Lerp(Color.gray, Color.white, ratio);
            }

            if (agent.propertyBlock == null)
                agent.propertyBlock = new MaterialPropertyBlock();

            renderer.GetPropertyBlock(agent.propertyBlock);
            agent.propertyBlock.SetColor("_BaseColor", energyColor);
            renderer.SetPropertyBlock(agent.propertyBlock);
        }

        public static void SetupEnergyGradientBasedOnTag(AgentController agent)
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys;

            if (agent.CompareTag("vole"))
            {
                colorKeys = new[] {
                    new GradientColorKey(Color.blue, 0f),
                    new GradientColorKey(Color.magenta, 1f)
                };
            }
            else if (agent.CompareTag("flower"))
            {
                colorKeys = new[] {
                    new GradientColorKey(Color.blue, 0f),
                    new GradientColorKey(Color.yellow, 1f)
                };
            }
            else
            {
                colorKeys = new[] {
                    new GradientColorKey(Color.gray, 0f),
                    new GradientColorKey(Color.white, 1f)
                };
            }

            GradientAlphaKey[] alphaKeys = {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };

            gradient.SetKeys(colorKeys, alphaKeys);
            agent.energyGradient = gradient;
        }

        public static void PrepareMaterialAndGradient(AgentController agent)
        {
            agent.bodyRenderer = agent.GetComponentInChildren<Renderer>();
            if (agent.bodyRenderer == null)
                return;

            if (agent.propertyBlock == null)
                agent.propertyBlock = new MaterialPropertyBlock();

            SetupEnergyGradientBasedOnTag(agent);
            UpdateVisualState(agent);
        }
    }
}
