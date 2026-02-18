using Microsoft.EntityFrameworkCore;

namespace backend.Infrastracture.Persistance;

public class BackendContext (DbContextOptions<BackendContext> options) : DbContext(options) {
    
}