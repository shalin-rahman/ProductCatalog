export interface Product {
  id: number;
  name: string;
  description?: string;
  price: number;
}

export interface ProductCreate {
  name: string;
  description?: string;
  price: number;
}

export interface ProductUpdate {
  name: string;
  description?: string;
  price: number;
}
