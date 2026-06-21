import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary shadow-sm">
      <div class="container">
        <a class="navbar-brand fw-bold" href="/"><i class="bi bi-box-seam me-2"></i>Product Catalog</a>
        <span class="navbar-text text-white-50 small">.NET 8 + Angular 19 Demo</span>
      </div>
    </nav>
    <main><router-outlet /></main>
    <footer class="text-center text-muted small py-4 mt-5 border-top">
      Product Catalog Demo &mdash; ASP.NET Core 8 &bull; Entity Framework Core &bull; Angular 19
    </footer>
  `
})
export class AppComponent { title = 'product-catalog-ui'; }
