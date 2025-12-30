import { apiRequest } from "./api-client";

export interface FieldOption {
  value: string;
  label: string;
}

export interface FieldDefinitionResponse {
  id: number;
  subcategoryId: number;
  name: string;
  label: string;
  key: string;
  type: string;
  isRequired: boolean;
  defaultValue?: string;
  options?: FieldOption[];
  min?: number;
  max?: number;
}

export interface CreateFieldDefinitionRequest {
  name: string;
  label: string;
  key: string;
  type: string;
  isRequired: boolean;
  defaultValue?: string;
  options?: FieldOption[];
  min?: number;
  max?: number;
}

export async function getFieldDefinitions(
  token: string,
  subcategoryId: number
): Promise<FieldDefinitionResponse[]> {
  try {
    return await apiRequest<FieldDefinitionResponse[]>(
      `/api/admin/subcategories/${subcategoryId}/fields`,
      {
        method: "GET",
        token,
        silent: false,
      }
    );
  } catch (error: any) {
    // Handle 404 as empty list (no fields defined yet)
    if (error?.status === 404) {
      console.log(`[getFieldDefinitions] 404 for subcategory ${subcategoryId} - returning empty list`);
      return [];
    }
    // Handle 500 errors that might be schema-related (e.g., missing DefaultValue column)
    if (error?.status === 500) {
      const errorBody = error?.body || {};
      const errorMessage = error?.message || "";
      const errorDetail = (errorBody as any)?.message || (errorBody as any)?.error || errorMessage;
      
      if (errorDetail.includes("DefaultValue") || 
          errorDetail.includes("no such column") ||
          errorDetail.includes("schema") ||
          errorDetail.includes("migration") ||
          errorDetail.includes("out of sync")) {
        console.warn(`[getFieldDefinitions] Database schema error for subcategory ${subcategoryId}:`, errorDetail);
        // Throw a user-friendly error that will be shown in the UI
        const friendlyError = new Error("خطای پایگاه داده: لطفاً سرور بک‌اند را راه‌اندازی مجدد کنید تا مایگریشن‌ها اعمال شوند.");
        (friendlyError as any).status = 500;
        (friendlyError as any).body = errorBody;
        throw friendlyError;
      }
    }
    throw error;
  }
}

export async function createFieldDefinition(
  token: string,
  subcategoryId: number,
  request: CreateFieldDefinitionRequest
): Promise<FieldDefinitionResponse> {
  return await apiRequest<FieldDefinitionResponse>(
    `/api/admin/subcategories/${subcategoryId}/fields`,
    {
      method: "POST",
      token,
      body: request,
      silent: false,
    }
  );
}

export async function updateFieldDefinition(
  token: string,
  subcategoryId: number,
  fieldId: number,
  request: Partial<CreateFieldDefinitionRequest>
): Promise<FieldDefinitionResponse> {
  return await apiRequest<FieldDefinitionResponse>(
    `/api/admin/subcategories/${subcategoryId}/fields/${fieldId}`,
    {
      method: "PUT",
      token,
      body: request,
      silent: false,
    }
  );
}

export async function deleteFieldDefinition(
  token: string,
  subcategoryId: number,
  fieldId: number
): Promise<void> {
  return await apiRequest<void>(
    `/api/admin/subcategories/${subcategoryId}/fields/${fieldId}`,
    {
      method: "DELETE",
      token,
      silent: false,
    }
  );
}
