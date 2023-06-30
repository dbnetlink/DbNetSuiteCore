﻿class EditColumn extends DbColumn{
    constructor(properties: EditColumnResponse, unmatched = false) {
        super();
        Object.keys(properties).forEach((key) => {
            if (properties[key as keyof EditColumnResponse] !== undefined)
                this[key as keyof EditColumnResponse] = properties[key as keyof EditColumnResponse] as any;
        });

        this.unmatched = unmatched;
    }
}