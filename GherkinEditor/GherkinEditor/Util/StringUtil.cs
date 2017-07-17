using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public static class StringUtil
    {
        /// <summary>
        /// Check if all characters are digits.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDigitsOnly(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;

            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if string contains Non-ASCII code
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool ContainsUnicodeCharacter(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            const int MaxAnsiCode = 255;
            return input.Any(c => c > MaxAnsiCode);
        }

        /// <summary>
        /// Find Nth occurrence of a character in a string
        /// </summary>
        /// <param name="s">input string</param>
        /// <param name="t">character to find</param>
        /// <param name="n">Nth occurrence</param>
        /// <returns>index of the character if exists, otherwise -1</returns>
        public static int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
