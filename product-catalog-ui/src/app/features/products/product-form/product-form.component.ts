import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './product-form.component.html'
})
export class ProductFormComponent implements OnInit {
  productForm!: FormGroup;
  isEditMode = false;
  productId: number | null = null;
  isSubmitting = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.productForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      price: [null, [Validators.required, Validators.min(0.01)]]
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.productId = Number(idParam);
      this.productService.getById(this.productId).subscribe({
        next: (p) => this.productForm.patchValue(p),
        error: () => { this.errorMessage = 'Product not found.'; }
      });
    }
  }

  get f() { return this.productForm.controls; }

  onSubmit(): void {
    if (this.productForm.invalid) { this.productForm.markAllAsTouched(); return; }
    this.isSubmitting = true;
    this.errorMessage = '';
    const val = this.productForm.value;

    if (this.isEditMode && this.productId !== null) {
      this.productService.update(this.productId, val).subscribe({
        next: () => this.router.navigate(['/products']),
        error: () => { this.errorMessage = 'Failed to update product.'; this.isSubmitting = false; }
      });
    } else {
      this.productService.create(val).subscribe({
        next: () => this.router.navigate(['/products']),
        error: () => { this.errorMessage = 'Failed to create product.'; this.isSubmitting = false; }
      });
    }
  }
}
