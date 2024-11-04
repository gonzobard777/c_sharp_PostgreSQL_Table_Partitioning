using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Puanson
{
    [Key] 
    public DateTime Date { get; set; }

    [Column(TypeName = "jsonb")] 
    public string? Data { get; set; }
}