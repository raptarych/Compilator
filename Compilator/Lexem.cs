using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    /// <summary>
    /// Сущность лексемы; содержит тип лексемы и ссылку на своё значение
    /// </summary>
    public class Lexem
    {
        public LexemType Key { get; set; }
        public byte ValuePtr { get; set; }

        public object GetValue()
        {
            switch (Key)
            {
                case LexemType.CONSTANT:
                    return CommonTables.Constants[ValuePtr];
                case LexemType.KEYWORD:
                    return CommonTables.Keywords[ValuePtr];
                case LexemType.IDENTIFIER:
                    return CommonTables.Identifiers[ValuePtr];
                case LexemType.OPERATION:
                    return CommonTables.Operations[ValuePtr];
                default:
                    return (char)ValuePtr;
            }
        }
    }
}
