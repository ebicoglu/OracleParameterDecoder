using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace OracleParameterDecoder
{
    public class Decoder
    {
        public static string DecodeMessage(string query)
        {
            const string parameterStart = "[Parameters=[";

            var queryLines = query.Split('\n').Select(x => x.Trim()).ToArray();
            var parametersIndex = queryLines[0].IndexOf(parameterStart, StringComparison.Ordinal);
            if (parametersIndex < 0)
            {
                return query;
            }

            var parametersArray = DecodeParameters(queryLines, parameterStart, parametersIndex);
            var parameters = ConvertParametersToDictionary(parametersArray);
            var queryWithoutParameters = RemoveFirstLine(query);
            queryWithoutParameters = ReplaceParameters(parameters, queryWithoutParameters);
            queryWithoutParameters = AddCursorDeclarations(queryWithoutParameters, parameters);

            return queryWithoutParameters;
        }

        private static string[] DecodeParameters(string[] queryLines, string parameterStart, int parametersIndex)
        {
            var firstLine = queryLines[0];
            firstLine = TrimStart(firstLine, parameterStart);
            var end = firstLine.IndexOf("], CommandType", StringComparison.Ordinal);

            var parameterText = queryLines[0].Substring(parametersIndex + parameterStart.Length);
            parameterText = ", " + parameterText.Substring(0, end);
            var parameterList = parameterText.Split(", :", StringSplitOptions.RemoveEmptyEntries);
            return parameterList;
        }

        private static OrderedDictionary ConvertParametersToDictionary(string[] parameterList)
        {
            var parametersWithoutDefitions = new OrderedDictionary();
            foreach (var parameter in parameterList)
            {
                if (parameter.Contains("cur"))
                {
                    Console.WriteLine("");
                }

                var paramWithoutDefinitions = Regex.Replace(parameter, @"\s(\((?>[^()]+|)*\))",
                    m => m.Value.Contains(" = ") ? string.Empty : m.Value
                );

                paramWithoutDefinitions = ":" + paramWithoutDefinitions;

                var equality = paramWithoutDefinitions.LastIndexOf('=');
                var key = paramWithoutDefinitions.Substring(0, equality);
                var value = paramWithoutDefinitions.Substring(equality + 1, paramWithoutDefinitions.Length - equality - 1);

                parametersWithoutDefitions.Add(key, value);
            }

            return parametersWithoutDefitions;
        }

        private static string ReplaceParameters(OrderedDictionary parametersWithoutDefitions, string queryWithoutParameters)
        {
            foreach (DictionaryEntry dictionaryEntry in parametersWithoutDefitions)
            {
                if (dictionaryEntry.Key.ToString().StartsWith(":p"))
                {
                    queryWithoutParameters = Regex.Replace(
                        queryWithoutParameters, @"(:p\d+|@__\w*?_\d+)",
                        dictionaryEntry.Value.ToString()
                    );
                }
                else if (dictionaryEntry.Key.ToString().StartsWith(":cur"))
                {
                    queryWithoutParameters = queryWithoutParameters.Replace(dictionaryEntry.Key.ToString(),
                        dictionaryEntry.Key.ToString().Substring(1)
                    );
                }
            }

            return queryWithoutParameters;
        }

        private static string AddCursorDeclarations(string queryWithoutParameters, OrderedDictionary parametersWithoutDefitions)
        {
            var beginIndex = queryWithoutParameters.IndexOf("BEGIN");
            foreach (DictionaryEntry dictionaryEntry in parametersWithoutDefitions)
            {
                if (dictionaryEntry.Key.ToString().StartsWith(":cur"))
                {
                    var cur = dictionaryEntry.Key.ToString().Substring(1);

                    queryWithoutParameters =
                        queryWithoutParameters.Insert(beginIndex - 1, cur + " sys_refcursor;" + Environment.NewLine);
                }
            }

            return queryWithoutParameters;
        }

        public static string RemoveFirstLine(string input)
        {
            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return string.Join(Environment.NewLine, lines.Skip(1));
        }

        public static string TrimStart(string str, string sStartValue)
        {
            if (str.StartsWith(sStartValue))
            {
                str = str.Remove(0, sStartValue.Length);
            }
            return str;
        }
    }
}