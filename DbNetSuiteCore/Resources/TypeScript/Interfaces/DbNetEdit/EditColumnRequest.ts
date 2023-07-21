interface EditColumnRequest extends GridColumnRequest
{
    editControlType?: string;
    pattern?: string;
    browse?: boolean;
    required?: boolean;
}