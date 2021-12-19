using System;
using System.Collections.Generic;
using FluentAssertions;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class DwcFormatterTests
    {        
        [Theory]     
        [InlineData("EKALLN", "EKALLN")] //DCS
        [InlineData("TvetaValsta", "TvetaValsta")] //STX
        [InlineData("Lövskog  SÖ", "Lövskog  SÖ")] //DEL
        [InlineData("Ösa 2:6, V", "Ösa 2:6, V")] //SS3
        [InlineData("Piút", "Piút")] //SCI
        [InlineData("utara", "utara")] //VTS
        [InlineData("sen 🤣 Efter", "sen  Efter")] //Emoji        
        [InlineData("Testar\tTab och\nnyrad", "Testar Tab och nyrad")]
        [InlineData("Testarnyrad\r\n", "Testarnyrad")]
        [InlineData("Testarnyrad\r\n\n", "Testarnyrad")]
        [InlineData("Testarnyrad\r\n\r\n", "Testarnyrad")]
        [InlineData("Testarnyrad\n", "Testarnyrad")]
        [InlineData("Testar\nnyrad\n", "Testar nyrad")]
        [InlineData("Testar\"citation\"", "Testar\"citation\"")]
        public void TestRemoveInvalidCharacters(
           string str,           
           string expectedCleanStr)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = DwcFormatter.RemoveIllegalCharacters(str);                

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(expectedCleanStr);            
        }
    }
}