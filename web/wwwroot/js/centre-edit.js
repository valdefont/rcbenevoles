function closePopup() {
    $('#communePopup').hide();
}

function showPopup(communes) {
    var $list = $('#communeList');
    $list.empty();

    $.each(communes, function (i, commune) {
        var item = $('<div>')
            .addClass('commune-item')
            .text(commune.nomCommune + ' (' + commune.codePostal + ')')
            .on('click', function () {
                $('#CommuneInput').val(commune.nomCommune);
                $('#CodePostal').val(commune.codePostal); // Update CodePostal if needed
                closePopup();
            });
        $list.append(item);
    });

    $('#communePopup').show();
}


$(document).ready(function () {
    $('#CodePostal').on('input', function () {
        var codePostal = $(this).val();

        if (codePostal.length >= 4 && codePostal.length <= 5) {
            $('#loader').show();

            $.ajax({
                url: getCommunesUrl,
                type: 'POST',
                data: { codePostal: codePostal },
                success: function (data) {
                    if (data.length === 1) {
                        $('#CommuneInput').val(data[0].nomCommune);
                        $('#CodePostal').val(data[0].codePostal);
                    } else if (data.length > 1) {
                        showPopup(data);
                    }
                },
                error: function () {
                    console.error("Erreur lors de la récupération des communes.");
                },
                complete: function () {
                    $('#loader').hide();
                }
            });
        }
    });
});
