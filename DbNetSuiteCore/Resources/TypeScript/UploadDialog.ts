class UploadDialog extends Dialog {
    parent: DbNetEdit;
    inputFile: HTMLInputElement;
    $apply: JQuery<HTMLButtonElement>;
    file: File | undefined;
    $uploadButton: JQuery<HTMLButtonElement> | undefined;
    $previewImage: JQuery<HTMLImageElement>;
    $editImage: JQuery<HTMLElement> | undefined;
    fileMetaData: FileMetaData | undefined
    $selectFilesButton: JQuery<HTMLButtonElement>;
    validExtensions: Array<string> = [];
    constructor(id: string, parent: DbNetEdit) {
        super(id);

        this.$dialog?.on("dialogopen", (event) => this.dialogOpened(event));
        this.parent = parent;
        this.inputFile = (this.$dialog?.find("input[type='file']") as JQuery<HTMLInputElement>)[0];
        this.$selectFilesButton = this.$dialog?.find("[button-type='uploadfile']") as JQuery<HTMLButtonElement>;
        this.$selectFilesButton.on("click", () => this.selectFile());
        $(this.inputFile).on("change", () => this.fileSelected());
        this.$apply = this.$dialog?.find("[button-type='apply']") as JQuery<HTMLButtonElement>;
        this.$apply.on("click", () => this.apply());
        this.$dialog?.find("[button-type='cancel']").on("click", () => this.close());
        this.$previewImage = this.$dialog?.find('img.preview') as JQuery<HTMLImageElement>;
        this.$previewImage.on("load", () => this.previewLoaded());
    }

    private dialogOpened(event: JQuery.TriggeredEvent): void {
        this.clear();
        const height = this.$dialog?.find('table.file-info').height() as number + 10;
        this.$dialog?.find('img.preview').height(height);
        this.sizeDialog();
        this.$selectFilesButton.trigger("click");
    }

    private sizeDialog(): void {
        const width = this.$dialog?.find("table").first().width() as number + 20;
        this.$dialog?.dialog("option", "width", width);
    }

    public show(event: JQuery.TriggeredEvent): void {
        this.$uploadButton = $(event.currentTarget as HTMLButtonElement);
        this.$editImage = (this.$uploadButton as JQuery<HTMLButtonElement>).closest("td").find("img,a");
        $(this.inputFile).attr("accept", this.$editImage.attr("accept") as string);
        this.validExtensions = (this.$editImage.attr("extensions") as string).toLowerCase().split(',');

        this.open();
    }

    private previewLoaded() {
        this.$previewImage.show();
        this.sizeDialog();
    }

    private selectFile(): void {
        $(this.inputFile).trigger("click");
    }

    private fileSelected(): void {
        const demoImage = this.$previewImage[0] as HTMLImageElement;
        this.file = this.inputFile.files?.item(0) as File;

        const ext = `.${this.file.name.split('.').pop()}`;

        if (this.validExtensions.indexOf(ext) == -1) {
            this.message("Selected file extension is not valid");
            this.clear();
            return;
        }

        this.fileMetaData = { fileName: this.file.name, size: this.file.size, contentType: this.file.type, lastModified: new Date(this.file.lastModified) } as FileMetaData;
        this.$dialog?.find("#name").val(this.file.name);
        this.$dialog?.find("#lastmodified").val(new Date(this.file.lastModified).toLocaleString());
        this.$dialog?.find("#type").val(this.file.type);
        this.$dialog?.find("#size").val(this.file.size);
        if ((this.file as File).type.startsWith("image/")) {
            this.renderFileAsImage(demoImage);
        }
        else {
            this.renderFileAsNonImage(demoImage);
        }
        this.$apply.prop("disabled", false);
        this.sizeDialog();
    }

    private clear(): void {
        this.$dialog?.find('input').val('');
        this.$previewImage.hide();
        this.$apply.prop("disabled", true);
    }

    private renderFileAsImage(img: HTMLImageElement) {
        const reader = new FileReader();
        reader.onload = function () {
            img.src = reader.result as string;
        }
        reader.readAsDataURL(this.file as File);
    }

    private renderFileAsNonImage(img: HTMLElement) {
        $(img).hide();
        $(img).parent().addClass("file");

        const fileName = (this.file as File).name;
        const ext = fileName.split('.').pop()?.toLowerCase();
        switch (ext) {
            case "csv":
            case "pdf":
               $(img).parent().addClass(ext);
                break;
            default:
                $(img).parent().addClass('default');
                break;
        }
        $(img).parent().attr("title", fileName);
    }

    private apply(): void {
        const $img = this.$editImage as JQuery<HTMLElement>;
        const img = $img.get(0) as HTMLElement;
        if ((this.file as File).type.startsWith("image/")) {
            this.renderFileAsImage(img as HTMLImageElement);
        }
        else {
            this.renderFileAsNonImage(img);
        }
        const file = new File([this.file as File], (this.file as File).name);
        this.parent.saveFile($img, file, this.fileMetaData);
        this.close();
    }
}