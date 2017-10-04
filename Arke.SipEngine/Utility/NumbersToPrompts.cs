using System;
using System.Collections.Generic;
using NLog;

namespace Arke.SipEngine.Utility
{
    public class NumbersToPrompts
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public List<string> GetPromptsForValue(decimal value)
        {
            if (value < -9999 || value > 9999)
                throw new ArgumentException("Values larger than 9999 or less than -9999 are not supported");
            var prompts = new List<string>();

            if (value == 0)
            {
                prompts.Add("0");
                prompts.Add("dollars");
                return prompts;
            }
            try
            {
                var valueAsString = value.ToString("###.##");
                var decimalPlace = valueAsString.IndexOf(".", StringComparison.Ordinal);

                if (value >= 2)
                {
                    AddPromptsForNumber(prompts,
                        decimalPlace > 0 ? valueAsString.Substring(0, decimalPlace) : valueAsString);
                    prompts.Add("dollars");
                }
                else
                {
                    prompts.Add("1");
                    prompts.Add("dollar");
                }

                if (decimalPlace > 0)
                {
                    if (prompts.Count > 0)
                        prompts.Add("and");
                    AddPromptsForDecimals(prompts, valueAsString.Substring(decimalPlace + 1).PadRight(2, '0'));
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error converting value to prompts");
                throw;
            }
            return prompts;
        }

        private void AddPromptsForDecimals(ICollection<string> prompts, string number)
        {
            if (number.Substring(0, 1) != "0")
                AddTensNumbersToList(prompts, number);
            else
                AddOnesNumbersToList(prompts, number);
            prompts.Add("cents");
        }

        private void AddOnesNumbersToList(ICollection<string> prompts, string number)
        {
            int value;
            bool result = Int32.TryParse(number, out value);
            if (!result)
            {
                value = 0;
            }
 
            prompts.Add(value.ToString());
        }

        private void AddTensNumbersToList(ICollection<string> prompts, string number)
        {
            var value = Convert.ToInt32(number);
            prompts.Add(value.ToString());
        }

        private void AddPromptsForNumber(ICollection<string> prompts, string number)
        {
            try
            {
                var isDone = false;
                var numberOfDigits = number.Length;
                var position = 0;
                var groupingPrompt = "";

                switch (numberOfDigits)
                {
                    case 1:
                        AddOnesNumbersToList(prompts, number);
                        isDone = true;
                        break;
                    case 2:
                        AddTensNumbersToList(prompts, number);
                        isDone = true;
                        break;
                    case 3:
                        position = (numberOfDigits % 3) + 1;
                        groupingPrompt = "hundred";
                        break;
                    case 4:
                        position = (numberOfDigits % 4) + 1;
                        groupingPrompt = "thousand";
                        break;
                }

                if (isDone) return;
                if (number.Substring(0, position) != "0" && number.Substring(position) != "0")
                {
                    if (groupingPrompt == "hundred")
                        prompts.Add(number.Substring(0, position) + "00");
                    else
                        prompts.Add(number.Substring(0, position) + "000");
                    AddPromptsForNumber(prompts, number.Substring(position));
                }
                else
                {
                    AddPromptsForNumber(prompts, number.Substring(0, position));
                    AddPromptsForNumber(prompts, number.Substring(position));
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error converting number to prompts");
                throw;
            }
        }
    }
}
