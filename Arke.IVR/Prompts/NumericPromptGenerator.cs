using System;
using System.Collections.Generic;
using System.Globalization;

namespace Arke.IVR.Prompts
{
    public class NumericPromptGenerator
    {
        public List<string> GetPromptsForNumber(double value)
        {
            var number = value.ToString(CultureInfo.CurrentCulture);
            return ChangeNumberToWords(number, false);
        }

        public List<string> GetPromptsForCurrency(double value)
        {
            var number = value.ToString(CultureInfo.CurrentCulture);
            return ChangeNumberToWords(number, true);
        }

        private List<string> ChangeNumberToWords(string number, bool isCurrency)
        {
            var prompts = new List<string>();
            var endString = "";

            var decimalPlace = number.IndexOf(".", StringComparison.Ordinal);
                
            if (decimalPlace > 0)
            {
                var wholeNumber = number.Substring(0, decimalPlace);
                prompts.AddRange(TranslateWholeNumber(wholeNumber));
                var points = number.Substring(decimalPlace + 1);
                if (Convert.ToInt32(points) > 0)
                {
                    var andString = isCurrency ? "and" : "point";
                    if (isCurrency)
                    {
                        endString = Convert.ToInt32(points) == 1 ? "cent" : "Cents";
                        prompts.Add(wholeNumber == "1" ? "Dollar" : "Dollars");
                    }
                    prompts.Add(andString);
                }
                prompts.AddRange(TranslateCents(points));
            }
            else
            {
                prompts.AddRange(TranslateWholeNumber(number));
                prompts.Add(number == "1" ? "Dollar" : "Dollars");
            }
            if (!string.IsNullOrEmpty(endString))
                prompts.Add(endString);
            return prompts;
        }

        private IEnumerable<string> TranslateWholeNumber(string wholeNumber)
        {
            var translatedPrompts = new List<string>();
            var dblAmt = Convert.ToDouble(wholeNumber);
            var isDone = false;

            if (dblAmt > 0)
            {
                var numDigits = wholeNumber.Length;
                var pos = 0;
                var place = "";
                switch (numDigits)
                {
                    case 1:
                        translatedPrompts.Add($"digits/{wholeNumber}");
                        isDone = true;
                        break;
                    case 2:
                        translatedPrompts.AddRange(TranslateTens(wholeNumber));
                        isDone = true;
                        break;
                    case 3:
                        pos = (numDigits%3) + 1;
                        place = "digits/hundred";
                        break;
                    case 4:
                    case 5:
                    case 6:
                        pos = (numDigits%4) + 1;
                        place = "digits/thousand";
                        break;
                    case 7:
                    case 8:
                    case 9:
                        pos = (numDigits%7) + 1;
                        place = "digits/million";
                        break;
                    default:
                        isDone = true;
                        break;
                }

                if (isDone)
                    return translatedPrompts;

                translatedPrompts.AddRange(TranslateWholeNumber(wholeNumber.Substring(0, pos)));
                if (!string.IsNullOrEmpty(place))
                    translatedPrompts.Add(place);
                translatedPrompts.AddRange(TranslateWholeNumber(wholeNumber.Substring(pos)));
            }
            return translatedPrompts;
        }

        private IEnumerable<string> TranslateTens(string digit)
        {
            var digt = Convert.ToInt32(digit);
            var prompts = new List<string>();
            switch (digt)
            {
                case 10:
                    prompts.Add("digits/10");
                    break;
                case 11:
                    prompts.Add("digits/11");
                    break;
                case 12:
                    prompts.Add("digits/12");
                    break;
                case 13:
                    prompts.Add("digits/13");
                    break;
                case 14:
                    prompts.Add("digits/14");
                    break;
                case 15:
                    prompts.Add("digits/15");
                    break;
                case 16:
                    prompts.Add("digits/16");
                    break;
                case 17:
                    prompts.Add("digits/17");
                    break;
                case 18:
                    prompts.Add("digits/18");
                    break;
                case 19:
                    prompts.Add("digits/19");
                    break;
                case 20:
                    prompts.Add("digits/20");
                    break;
                case 30:
                    prompts.Add("digits/30");
                    break;
                case 40:
                    prompts.Add("digits/40");
                    break;
                case 50:
                    prompts.Add("digits/50");
                    break;
                case 60:
                    prompts.Add("digits/60");
                    break;
                case 70:
                    prompts.Add("digits/70");
                    break;
                case 80:
                    prompts.Add("digits/80");
                    break;
                case 90:
                    prompts.Add("digits/90");
                    break;
                default:
                    if (digt > 0)
                    {
                        prompts.AddRange(TranslateTens(digit.Substring(0, 1) + "0"));
                        prompts.Add($"digits/{digit.Substring(1)}");
                    }
                    break;
            }
            return prompts;
        }

        private IEnumerable<string> TranslateCents(string cents)
        {
            var prompts = new List<string>();

            if (cents.Length > 2)
                cents = cents.Substring(0, 2);

            if (cents.Equals("0"))
                prompts.Add("digits/0");
            else
                prompts.AddRange(TranslateTens(cents));

            return prompts;
        }
    }
}
