export interface User {
    userId: number
    name: string
    role: string
    email: string
    phone: string
    status: string
  }
  
  export interface UserFormData {
    userId?: number
    name: string
    role: string
    email: string
    phone: string
    status: string
  }
  
export type Provider = {
  providerId: number;
  npi: string;
  name: string;
  taxonomy: string;
  telehealthEnrolled: number;
  contactInfo: string;
  status: string;
};

export type ProviderFormValues = {
  name: string;
  npi: string;
  taxonomy: string;
  telehealthEnrolled: number;
  contactInfo: string;
  status: string;
};

export type Payer = {
  payerId: number;
  name: string;
  payerCode: string;
  clearinghouseCode: string;
  contactInfo: string;
  status: {props: {status: string}};
};

export type PayerFormValues = {
  Name: string;
  PayerCode: string;
  ClearinghouseCode: string;
  ContactInfo: string;
  Status: string;
};