using System.ComponentModel.DataAnnotations.Schema;
using Domain.Contract;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey(nameof(Id), nameof(Date))]
public class Puanson : IEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    [Column(TypeName = "jsonb")] 
    public string? Data { get; set; }
}