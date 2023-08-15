class ImageViewer extends Dialog {
    $image: JQuery<HTMLImageElement>;
    $download: JQuery<HTMLAnchorElement>;
    fileName = "";
    constructor(id: string) {
        super(id);
        this.$image = this.$dialog?.find("img.main-image") as JQuery<HTMLImageElement>;
        this.$download = this.$dialog?.find("a.download-link") as JQuery<HTMLAnchorElement>;
        this.button("download").on("click", () => this.download());
        this.$image.on("load", () => this.open())
    }

    public show($img: JQuery<HTMLImageElement>) {
        this.$image.attr("src", $img.attr("src") as string)
        this.fileName = $img.data("filename")
    }

    private download() {
        const link = this.$download.get(0) as HTMLAnchorElement ;
        link.href = this.$image.attr("src") as string;
        link.download = this.fileName;
        link.click();
    }
}