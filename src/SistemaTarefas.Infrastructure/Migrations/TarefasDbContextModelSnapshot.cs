using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SistemaTarefas.Infrastructure.Persistencia;

#nullable disable

namespace SistemaTarefas.Infrastructure.Migrations;

[DbContext(typeof(TarefasDbContext))]
partial class TarefasDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");
        modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 64);
    }
}
