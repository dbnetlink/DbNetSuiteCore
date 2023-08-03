"use strict";
class UploadDialog extends Dialog {
    constructor(id, parent) {
        var _a, _b, _c, _d, _e, _f;
        super(id);
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        this.inputFile = ((_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("input[type='file']"))[0];
        this.$selectFilesButton = (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("[button-type='uploadfile']");
        this.$selectFilesButton.on("click", () => this.selectFile());
        $(this.inputFile).on("change", () => this.fileSelected());
        this.$apply = (_d = this.$dialog) === null || _d === void 0 ? void 0 : _d.find("[button-type='apply']");
        this.$apply.on("click", () => this.apply());
        (_e = this.$dialog) === null || _e === void 0 ? void 0 : _e.find("[button-type='cancel']").on("click", () => this.close());
        this.$previewImage = (_f = this.$dialog) === null || _f === void 0 ? void 0 : _f.find('img.preview');
        this.$previewImage.on("load", () => this.previewLoaded());
    }
    dialogOpened(event) {
        var _a, _b;
        this.clear();
        const height = ((_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find('table.file-info').height()) + 10;
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find('img.preview').height(height);
        this.sizeDialog();
        this.$selectFilesButton.trigger("click");
    }
    sizeDialog() {
        var _a, _b;
        const width = ((_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("table").first().width()) + 20;
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.dialog("option", "width", width);
    }
    show(event) {
        this.$uploadButton = $(event.currentTarget);
        this.$editImage = this.$uploadButton.closest("td").find("img");
        $(this.inputFile).attr("accept", this.$editImage.attr("accept"));
        this.open();
    }
    previewLoaded() {
        this.$previewImage.show();
        this.sizeDialog();
    }
    selectFile() {
        $(this.inputFile).trigger("click");
    }
    fileSelected() {
        var _a, _b, _c, _d, _e;
        const demoImage = this.$previewImage[0];
        this.file = (_a = this.inputFile.files) === null || _a === void 0 ? void 0 : _a.item(0);
        this.fileMetaData = { fileName: this.file.name, size: this.file.size, contentType: this.file.type, lastModified: new Date(this.file.lastModified) };
        (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("#name").val(this.file.name);
        (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("#lastmodified").val(new Date(this.file.lastModified).toLocaleString());
        (_d = this.$dialog) === null || _d === void 0 ? void 0 : _d.find("#type").val(this.file.type);
        (_e = this.$dialog) === null || _e === void 0 ? void 0 : _e.find("#size").val(this.file.size);
        if (this.file.type.startsWith("image/")) {
            this.renderFileAsImage(demoImage);
        }
        else {
            this.renderFileAsNonImage(demoImage);
        }
        this.$apply.prop("disabled", false);
        this.sizeDialog();
    }
    clear() {
        var _a;
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find('input').val('');
        this.$previewImage.hide();
        this.$apply.prop("disabled", true);
    }
    renderFileAsImage(img) {
        const reader = new FileReader();
        reader.onload = function () {
            img.src = reader.result;
        };
        reader.readAsDataURL(this.file);
    }
    renderFileAsNonImage(img) {
        var _a;
        $(img).hide();
        $(img).parent().addClass("file");
        const fileName = this.file.name;
        const ext = (_a = fileName.split('.').pop()) === null || _a === void 0 ? void 0 : _a.toLowerCase();
        switch (ext) {
            case "csv":
                $(img).parent().addClass(ext);
                break;
            default:
                $(img).parent().addClass('default');
                break;
        }
        $(img).parent().attr("title", fileName);
    }
    apply() {
        const $img = this.$editImage;
        if (this.file.type.startsWith("image/")) {
            this.renderFileAsImage($img.get(0));
        }
        const file = new File([this.file], this.file.name);
        this.parent.saveFile($img, file, this.fileMetaData);
        this.close();
    }
}
