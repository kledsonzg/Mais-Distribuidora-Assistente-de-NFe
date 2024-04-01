let form = document.querySelector('#volume-identification-form');
let insertNfeBtn = document.querySelector('#btn-add-nfe');
let resetBtn = document.querySelector('#btn-reset');
let importBtn = document.querySelector('#btn-import-nfs');
let nfeInput = document.querySelector('#form-input-nfe');
let nfeList = document.querySelector('#nfe-list ol');

resetBtn.addEventListener('click', () => {
    nfeInput.value = '';
    let items = getInvoiceElements();

    items.forEach(item => { item.remove(); } );
    document.querySelector('.nfe-list-empty-alert').classList.toggle('hidden', false);
} );

insertNfeBtn.addEventListener('click', () => {
    let value = nfeInput.value;
    if(value.trim().length === 0 || value.trim().length > 9){
        alert(decodeURI('O campo \'NFE\' deve conter 1-9 dígitos.') );
        return;
    }
    if(getInvoiceNumbers().includes(value) ){
        alert(decodeURI('Esta NFe já está na lista de NF\'s a serem importadas.') );
        return;
    }

    let model = document.querySelector('.hidden.li-model');
    let clone = model.cloneNode(true);

    clone.classList.toggle('hidden', false);
    clone.classList.toggle('li-model', false);
    clone.querySelector('span').innerText = value;
    clone.querySelector('button').addEventListener('click', () => {
        clone.remove();

        if(getInvoiceElements().length === 0)
            document.querySelector('.nfe-list-empty-alert').classList.toggle('hidden', false);
    } );

    nfeInput.value = '';
    nfeList.appendChild(clone);
    document.querySelector('.nfe-list-empty-alert').classList.toggle('hidden', true);
} );

nfeInput.addEventListener('input', () => {
    nfeInput.value = nfeInput.value.replaceAll(',', '').replaceAll('\\.', '');
} );
nfeInput.addEventListener('keydown', (eventInfo) => {
    if(eventInfo.key !== 'Enter')
        return;
    insertNfeBtn.click();
} );

importBtn.addEventListener('click', () => {
    importBtn.disabled = true;
    let nfs = getInvoiceNumbers();
    if(nfs.length === 0){
        alert(decodeURI('Insira pelo menos uma nota fiscal na lista antes de prosseguir com a requisição!') );
        importBtn.disabled = false;
        return;
    }

    let req = new XMLHttpRequest();
    req.open('POST', '/import-nfs');
    req.addEventListener('loadend', () => {
        importBtn.disabled = false;
        onInvoiceRequestLoadEnd(req);
    } );
    req.send(JSON.stringify(nfs) );
} );

function getInvoiceElements(){
    return Array.from(nfeList.querySelectorAll('li') );
}

function getInvoiceNumbers(){
    let items = Array.from(nfeList.querySelectorAll('li span') );
    let result = [];
    items.forEach(item => {
        result.push(item.innerText);
    } )

    return result;
}

//form.classList.toggle('hidden', false);