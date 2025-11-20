using _1_Calculator.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _1_Calculator.Data
{
    public class DataInputVariant
    {
        [Key]
        public int ID_DataInputVariant { get; set; }
        public double Operand_1 { get; set; }
        public double Operand_2 { get; set; }
        public Operation Type_operation { get; set; }
        [Column(TypeName = "varchar(128)")]
        public string? Result { get; set; }
    }
}
