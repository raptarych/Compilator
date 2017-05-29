using System;
using System.Collections.Generic;
using System.Linq;

namespace FiniteStateMachine
{
    class FiniteStates
    {
        private static Dictionary<string, Dictionary<string,string>> PossibleStates;

        public static void SetTable(Dictionary<string, Dictionary<string, string>> table) => PossibleStates = table;

        public static string CurrentFileName { get; set; }

        public static Dictionary<string, Dictionary<string, string>> GetTable => PossibleStates;

        /// <summary>
        /// Прогнать через конечный автомат цепочку 
        /// </summary>
        public static void ProcessCode(string input)
        {
            if (PossibleStates == null)
                throw new Exception("Finite state machine wasn't defined");
            var queue = new Queue<char>(input.ToCharArray()); //LIFO

            var currentState = PossibleStates.FirstOrDefault().Key;

            while (queue.Any())
            {
                var currentChar = queue.Dequeue();
                var currentType = currentChar.ToString();

                if (!PossibleStates.FirstOrDefault().Value.ContainsKey(currentType)) throw new Exception($"Invalid symbol: {currentType}");
                currentState = PossibleStates[currentState][currentType];
            }

            var result = PossibleStates[currentState]["Output"] == "1";
            Console.WriteLine(result ? "Validated" : "Not validated");
        }

        /// <summary>
        /// Вывод информации о текущей группировке состояний (или если математически - классы эквивалентности)
        /// </summary>
        public static string GroupingInfo(List<IGrouping<string, KeyValuePair<string, Dictionary<string, string>>>> grouping)
            => string.Join(",", grouping.Select(group => $"{{{string.Join(",", group.Select(elem => elem.Key))}}}"));

        public static void FindUsedStates(List<string> usedStates, string currentState)
        {
            foreach (var state in PossibleStates[currentState])
            {
                if (!usedStates.Contains(state.Value) && state.Key != "Output")
                {
                    usedStates.Add(state.Value);
                    FindUsedStates(usedStates, state.Value);
                }
            }
        }

        /// <summary>
        /// Метод минимизации, возвращает false если автомат уже дальше некуда минимизировать
        /// </summary>
        public static bool MinimizeAutomat()
        {
            // 1) Недостижимые состояния
            // 1.1) Достижимые состояния
            var usedStates = new List<string>();
            var currentState = PossibleStates.FirstOrDefault().Key;
            FindUsedStates(usedStates, currentState);

            // 1.2) Вычитаем из множества всех состояний множество достижимых состояний и результат удаляем из состояний
            var unusedStates = PossibleStates.Where(state => !usedStates.Contains(state.Key) && state.Key != "Output").Select(state => state.Key).ToList();
            unusedStates.ForEach(state =>
            {
                PossibleStates.Remove(state);
                Console.WriteLine($"Deleted unreachable state: {state}");
            });

            //Формирование алфавита символов
            var alfabet = PossibleStates.FirstOrDefault()
                .Value.Select(val => val.Key)
                .Where(val => val != "Output")
                .ToList();
            //  2) Группировка для удаления эквивалентных состояний
            //  2.1) 0-эквивалентность
            var grouping = PossibleStates.GroupBy(elem => elem.Value["Output"]).ToList();
            Console.WriteLine($"Current grouping: {GroupingInfo(grouping)}");

            //  2.2) 1-эквивалентность
            foreach (var symbol in alfabet)
            {
                grouping = PossibleStates.GroupBy(arg =>
                {
                    var currentGroup = grouping.FirstOrDefault(gr => gr.Any(grElem => grElem.Key == arg.Key));
                    var currentGroupName = currentGroup?.Key;
                    var valid = PossibleStates[arg.Value[symbol]]["Output"];
                    return $"{currentGroupName}_{symbol}{valid}";
                }).ToList();
                Console.WriteLine($"Current grouping: {GroupingInfo(grouping)}");
            }

            //  2.3) 2-эквивалентность
            grouping = PossibleStates.GroupBy(arg =>
            {
                var currentGroup = grouping.FirstOrDefault(gr => gr.Any(state => state.Key == arg.Key));
                var currentGroupName = currentGroup?.Key;
                var returnString = $"{currentGroupName}_";
                foreach (var symbol in alfabet)
                {
                    var group2 = grouping.FirstOrDefault(gr => gr.Any(state => state.Key == arg.Value[symbol]))?.Key;
                    returnString += $"{symbol}{group2}";
                }
                return returnString;
            })
            .ToList();
            Console.WriteLine($"Current grouping: {GroupingInfo(grouping)}");

            if (grouping.All(group => group.Count() <= 1))
            {
                Console.WriteLine("Automat already minimized");
                return false;
            }
            Console.WriteLine($"Found similar states: {string.Join(",", grouping.Where(group => group.Count() > 1).Select(group => $"{{{string.Join(",", group.Select(elem => elem.Key))}}}"))}");

            //3) Замена эквивалентных состояний

            foreach (var group in grouping)
            {
                var statesToDelete = new List<string>();
                if (group.Count() <= 1) continue;
                var statesToMerge = group.Select(state => state.Key).ToList();
                var finalState = statesToMerge.FirstOrDefault();
                statesToMerge.Where(state => state != finalState).ToList().ForEach(state => statesToDelete.Add(state));
                var listToReplace = new List<string[]>();

                foreach (var state in PossibleStates)
                {
                    
                    foreach (var stateLink in state.Value)
                    {
                        if (stateLink.Key == "Output") continue;
                        if (statesToMerge.Contains(stateLink.Value) && finalState != stateLink.Value)
                            listToReplace.Add(new[] { state.Key, stateLink.Key, finalState });
                    }
                        
                }
                foreach (var item in listToReplace) PossibleStates[item[0]][item[1]] = item[2];
                foreach (var item in statesToDelete) PossibleStates.Remove(item);
            }
            
            
            Console.WriteLine("Successfully minimized!");
            return true;
        }

        
        [Obsolete]
        public static void EchoError(int errorCharIndex, string input, string message, int line)
        {
            Console.WriteLine(input);
            var pointer = "";
            for (int i = 0; i < errorCharIndex - 1; i++) pointer += " ";
            pointer += "^";
            Console.WriteLine(pointer);
            throw new Exception($"Compilation error at line {line} symbol {errorCharIndex}: {message}");
        }
    }
}
