$(document).ready(function () {
    /* must select element by id = Img */
    $('#Img').on('change', function () {
        $('.cover-preview').attr('src', window.URL.createObjectURL(this.files[0])).removeClass('d-none');
    });
    
});