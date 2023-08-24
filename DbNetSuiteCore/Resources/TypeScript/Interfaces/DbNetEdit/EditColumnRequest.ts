interface EditColumnRequest extends GridColumnRequest
{
    editControlType?: string;
    pattern?: string;
    required?: boolean;
    autoIncrement?: boolean;
    readOnly?: boolean;
}