import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Product } from '../../../shared/models/product.model';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './product-list.component.html'
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(private productService: ProductService) {}

  ngOnInit(): void { this.loadProducts(); }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getAll().subscribe({
      next: (data) => { this.products = data; this.isLoading = false; },
      error: (err) => { this.errorMessage = 'Failed to load products. Is the API running?'; this.isLoading = false; console.error(err); }
    });
  }

  deleteProduct(id: number, name: string): void {
    if (!confirm(`Are you sure you want to delete "${name}"?`)) return;
    this.productService.delete(id).subscribe({
      next: () => { this.products = this.products.filter(p => p.id !== id); },
      error: () => { this.errorMessage = 'Failed to delete product.'; }
    });
  }
}
