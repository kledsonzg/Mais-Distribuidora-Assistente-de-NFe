let floatingRightColumnn = document.querySelector('#column-right');
let resultContainer = document.querySelector('#result-container-body');
let resultGrid = document.querySelector('#result-container');
let floatingMsg = document.querySelector('#floating-selected-volume-info');

let floatingWindow = document.querySelector('#floating-window');
let floatingWindowCloseBtn = floatingWindow.querySelector('#close-container img');

let pressingCtrl = false;

let replaceContentBtn = document.querySelector('#btn-apply-replacement');

let hideRightColumnBtn = document.querySelector('#btn-hide-column-right');
let insertVolumeBtn = document.querySelector('#btn-insert-new-volume');
let generateBtn = document.querySelector('#btn-generate');

let products = [];
let volumes = [];
let selectedVolume = null;

generateBtn.addEventListener('click', () => { generateIdentifications(); } );

floatingWindowCloseBtn.addEventListener('click', () => {
    floatingWindow.classList.toggle('hidden', true);
})

replaceContentBtn.addEventListener('click', () => {
    let searchContentInput = replaceContentBtn.parentElement.querySelector('#input-search-string');
    let replaceContentInput = replaceContentBtn.parentElement.querySelector('#input-replace-string');

    let searchContent = searchContentInput.value;
    let replaceContent = replaceContentInput.value;

    searchContentInput.value = '';
    replaceContentInput.value = '';

    let contentElements = Array.from(resultGrid.querySelectorAll('.input-nf-content-product') );
    let elementsCount = 0;
    contentElements.forEach(element => {
        if(element.value.includes(searchContent) )
            elementsCount++;
        element.value = element.value.replaceAll(searchContent, replaceContent);
        onProductNameInputChange(element);
    } );

    alert(`Foi feita a alteração em ${elementsCount} nome(s) de produto(s).`);
} );

hideRightColumnBtn.addEventListener('click', () => {
    onKeyDown({key: 'control'} );
    onKeyDown({key: 'm'} );
    onKeyUp({key: 'control'} );
} );

insertVolumeBtn.addEventListener('click', () => {
    let container = floatingRightColumnn.querySelector('#btns-volumes');

    let element = document.createElement('img');
    element.src = '../style/images/box-open.png';
    
    let index = volumes.length + 1;
    element.alt = `Volume ${index}`;
    element.title = `Caixa Identificada ${index < 10 ? `0${index}`: index}`;

    let volume = {
        element: element,
        index: index,
        contents: []
    };
    volumes.push(volume);

    element.addEventListener('click', () => {
        let isFirstClick = element.classList.contains('volume-selected') === false;

        unselectVolumes();

        element.classList.toggle('volume-selected', true);
        selectedVolume = volumes.indexOf(volume);

        floatingMsg.classList.toggle('hidden', false);
        let floatingContentElement = floatingMsg.querySelector('span');
        floatingContentElement.innerText = `INSERINDO NO VOLUME: ${volumes[selectedVolume].index < 10 ? `0${volumes[selectedVolume].index}` : volumes[selectedVolume].index}`;

        if(!isFirstClick){
            floatingWindow.classList.toggle('hidden', false);
            showVolumeContent(volume);
        }
    } )

    container.appendChild(element);
} );

floatingMsg.addEventListener('mouseenter', () => {
    let floatingTop = floatingMsg.classList.contains('float-top');

    floatingMsg.classList.toggle('float-top', !floatingTop);
    floatingMsg.style = !floatingTop ? 'top: 15%;' : '';
} );

document.addEventListener('keydown', (eventInfo) => {
    onKeyDown(eventInfo);
} );

document.addEventListener('keyup', (eventInfo) => {
    onKeyUp(eventInfo);
} );

function onKeyUp(eventInfo){
    //console.log(eventInfo.key);
    if(resultContainer.classList.contains('hidden') )
        return;
    if(eventInfo.key.toLowerCase() !== 'control')
        return;

    pressingCtrl = false;
}

function onKeyDown(eventInfo){
    //console.log(eventInfo.key);
    if(resultContainer.classList.contains('hidden') )
        return;

    switch(eventInfo.key.toLowerCase() ){
        case 'm':
            if(!pressingCtrl)
                return;

            floatingRightColumnn.classList.toggle('hidden', !floatingRightColumnn.classList.contains('hidden') );
            resultGrid.classList.toggle('max-width', floatingRightColumnn.classList.contains('hidden') );
            floatingMsg.classList.toggle('max-width', floatingRightColumnn.classList.contains('hidden') );

            break;
        case 'control':
            pressingCtrl = true;
            break;
        case 'delete':
            if(selectedVolume === null)
                return;

            deleteVolume(selectedVolume);
            break;
        case 'escape':
            if(selectedVolume === null)
                return;

            unselectVolumes();
            break;
        default:
            return;
    }
}

function unselectVolumes(){
    let container = floatingRightColumnn.querySelector('#btns-volumes');
    let volumeElements = container.querySelectorAll('img');

    volumeElements.forEach(volumeElement => {
        volumeElement.classList.toggle('volume-selected', false);
    } );

    floatingMsg.classList.toggle('hidden', true);
    floatingWindow.classList.toggle('hidden', true);
    selectedVolume = null;
}

function showVolumeContent(volume){
    Array.from(floatingWindow.querySelectorAll('.volume-content-row:not(.model):not(.hidden)') ).forEach(element => {
        element.remove(); 
    } );

    let volumeContentsContainer = floatingWindow.querySelector('#volume-content-result-container');
    let titleElement = floatingWindow.querySelector('#volume-content-header h3');
    titleElement.innerText = `CAIXA IDENTIFICADA: ${volume.index < 10 ? `0${volume.index}` : `${volume.index}`}`;

    let rowModel = floatingWindow.querySelector('.volume-content-row.model');
    
    volume.contents.forEach(content => {
        let clone = rowModel.cloneNode(true);
        clone.classList.toggle('model', false);
        clone.classList.toggle('hidden', false);

        let nameElement = clone.querySelector('.volume-content-column-name');
        nameElement.innerText = content.content;

        let quantityElement = clone.querySelector('.volume-content-column-quantity');
        quantityElement.innerText = `${content.quantity}UN`;

        let deleteElement = clone.querySelector('.volume-content-btn-delete');
        deleteElement.addEventListener('click', () => {
            deleteContentFromVolume(volume, content);
            clone.remove();
            sortContentElements();
        } );

        volumeContentsContainer.appendChild(clone);
    } );
}

function deleteVolume(index){
    if(volumes[index] === undefined || floatingRightColumnn.classList.contains('hidden') )
        return;
    let volume = volumes[index];
    let contents = [];

    volume.contents.forEach(content => {
        contents.push(content);
    } );
    contents.forEach(content => {
        deleteContentFromVolume(volume, content);
    } );

    volumes.splice(volumes.indexOf(volume), 1);
    volume.element.remove();
    
    updateVolumesIndexes();
    sortContentElements();
    unselectVolumes();
}

function updateVolumesIndexes(){
    volumes.forEach(volume => {
        volume.index = volumes.indexOf(volume) + 1;

        volume.element.alt = `Volume ${volume.index}`;
        volume.element.title = `Caixa Identificada ${volume.index < 10 ? `0${volume.index}`: volume.index}`;
    } )
}

function deleteContentFromVolume(volume, content){
    let quantityElement = content.inputElement.parentElement.querySelector('.input-nf-content-quantity');
        
    content.originalProduct.quantity += content.quantity;     
    quantityElement.value = content.originalProduct.quantity;
    if(content.originalProduct.quantity > 0){
        let insertBtn = quantityElement.parentElement.querySelector('.btn-nf-content-add');
        insertBtn.disabled = false;
        
        let inputs = Array.from(quantityElement.parentElement.querySelectorAll('input') );
        inputs.forEach(input => {
            input.classList.toggle('content-inserted', false);

            if(input.classList.contains('content-quantity-alert') && parseInt(input.value) >= 0)
                input.classList.toggle('content-quantity-alert', false);
        } );
    }

    volume.contents.splice(volume.contents.indexOf(content), 1);
}

function showHideForm(toHide){
    if(typeof(toHide) !== 'boolean')
        return;

    form.classList.toggle('hidden', toHide);
}

function showHideResultContainer(toHide){
    if(typeof(toHide) !== 'boolean')
        return;

    resultContainer.classList.toggle('hidden', toHide);
}

function onInvoiceRequestLoadEnd(request){
    if(request.status !== 200)
    {
        alert('Ocorreu um erro durante a requisição ao servidor.');
        return;
    }

    let json = JSON.parse(request.responseText);
    let volumesQuantity = 0;
    json.forEach(obj => {
        obj.Products.forEach(product => {
            let item = {
                name: product.Name,
                quantity: product.Quantity,
                nf: parseInt(obj.NumberCode)
            }

            products.push(item);
        } )

        volumesQuantity += parseInt(obj.Volumes);
    } );

    products = products.sort((a, b) => a.nf - b.nf < 0);
    console.log(products);

    showHideForm(true);
    showHideResultContainer(false);

    loadResult();

    addVolumes(volumesQuantity);
    if(volumesQuantity > 0)
        alert(`${volumesQuantity} volume(s) foram adicionados automaticamente na lista.`);
}

function addVolumes(volumesQuantity){
    for(let i = 0; i < volumesQuantity; i++){
        // Simula um clique no botão de adicionar um volume.
        insertVolumeBtn.click();
    }
}

function applyMaxCharsReached(element, toApply){
    if(typeof(toApply) !== 'boolean')
        return;
    if(toApply === true){
        element.title = 'É recomendado alterar o nome deste produto para tentar diminuir a quantidade de caracteres';
        element.classList.toggle('row-characters-max-length-reached', true);
        return;
    }
    if(element.getAttribute('title') !== null)
        element.removeAttribute('title');

    element.classList.toggle('row-characters-max-length-reached', false);
}

function onProductNameInputChange(element){
    if(`${element.value} ${element.parentElement.querySelector('.input-nf-content-quantity').value}UN`.length >= 45)
        applyMaxCharsReached(element, true);
    else applyMaxCharsReached(element, false);
}

function sortContentElements(){
    let elements = Array.from(document.querySelectorAll('.nf-content') );
    let contentsIn = elements.filter(element => element.querySelector('input').classList.contains('content-inserted') );
    let contentsOut = elements.filter(element => !element.querySelector('input').classList.contains('content-inserted') );

    let contents = [];
    contentsIn.forEach(content => { 
        contents.push( 
            { 
                element: content, 
                parent: content.parentElement 
            } 
        ); 
    } );
    contentsOut.forEach(content => { 
        contents.push( 
            { 
                element: content, 
                parent: content.parentElement 
            } 
        ); 
    } );

    elements.forEach(element => { element.remove(); } );

    contents.forEach(content => { 
        content.parent.appendChild(content.element);
    } );
}

function loadResult(){
    let nfs = [];
    
    for(let i = 0; i < products.length && products.length > 0; i++){
        let product = products[i];
        if(nfs.includes(product.nf) )
            continue;

        nfs.push(product.nf);
    }

    nfs.forEach(nf => {
        let productsFiltered = products.filter(product => product.nf === nf);

        let model = resultGrid.querySelector('.nf-content-container.model');
        let productRowModel = model.querySelector('.nf-content');
        let clone = model.cloneNode(true);
        let productElements = Array.from(clone.querySelectorAll('.nf-content') );
        productElements.forEach(element => {
            element.remove();
        } );

        clone.classList.toggle('model', false);
        clone.classList.toggle('hidden', false);

        clone.querySelector('.nf-content-title h2').innerText = `NOTA FISCAL: ${nf}`;
        productsFiltered.forEach(product => {
            let rowClone = productRowModel.cloneNode(true);
            let inputName = rowClone.querySelector('.input-nf-content-product');
            let inputQuantity = rowClone.querySelector('.input-nf-content-quantity');
            let btnInsert = rowClone.querySelector('.btn-nf-content-add');
            
            if(`${product.name} ${product.quantity}UN`.length >= 45)
                applyMaxCharsReached(inputName, true);

            inputName.addEventListener('input', () => {
                onProductNameInputChange(inputName);
            } );
            inputQuantity.addEventListener('input', () => {
                inputQuantity.value = inputQuantity.value.replaceAll(',', '').replaceAll('\\.', '');
            } );

            inputName.value = product.name;
            inputQuantity.value = product.quantity.toString();

            btnInsert.addEventListener('click', () => {
                let quantity = parseInt(inputQuantity.value);
                if(quantity <= 0)
                {
                    alert('A quantidade precisa ser superior a 0!');
                    return;
                }

                if(selectedVolume == null)
                {
                    alert(`Você precisa selecionar o volume primeiro antes de tentar inserir conteúdo!${floatingRightColumnn.classList.contains('hidden') ? '\nPressione \'CTRL + M\' para visualizar o menu de volumes.' : ''}`);
                    return;
                }
                let userAddedMoreThanExists = false;
                if(quantity > product.quantity){
                    userAddedMoreThanExists =  confirm(`Há apenas \'${product.quantity}\' unidade(s) deste produto e você está tentando adicionar \'${quantity}\'. Deseja continuar?`);

                    if(!userAddedMoreThanExists)
                        return;
                }

                let content = {
                    inputElement: inputName,
                    content: inputName.value,
                    nf: nf,
                    originalProduct: product,
                    quantity: quantity
                };

                volumes[selectedVolume].contents.push(content);

                product.quantity -= quantity;
                inputQuantity.value = product.quantity.toString();

                if(product.quantity <= 0){
                    btnInsert.disabled = true;
                    inputName.classList.toggle('content-inserted', true);
                    inputQuantity.classList.toggle('content-inserted', true);
                }
                if(userAddedMoreThanExists){
                    inputQuantity.classList.toggle('content-quantity-alert', true);
                }

                sortContentElements();   
            } );

            clone.appendChild(rowClone);
        } );

        resultGrid.appendChild(clone);
    } );
}

function generateIdentifications(){
    generateBtn.disabled = true;

    let compactVolumes = [];
    let nfs = [];
    volumes.forEach(volume => {
        let compactVolume = {
            products: [],
            index: -1
        };

        compactVolume.index = volume.index;
        volume.contents.forEach(content => {
            compactVolume.products.push(`${content.content} ${content.quantity}UN`);

            if(!nfs.includes(content.nf) )
                nfs.push(content.nf);
        } );
        compactVolumes.push(compactVolume);
    } );


    let requestObj = {
        nfs: '',
        volumes: compactVolumes
    }

    nfs.forEach(nf => {
        requestObj.nfs += nf.toString();

        if(nfs.indexOf(nf) !== nfs.length - 1)
            requestObj.nfs += ', ';
    } );

    let req = new XMLHttpRequest();
    req.open('POST', '/generateVolumeIdentification');
    req.addEventListener('loadend', () => {
        generateBtn.disabled = true;

        if(req.status === 200)
            alert('A requisição para gerar as caixas identificadas foi enviada com sucesso!\nO sistema tentará criar e abrir o arquivo gerado.');
    } )
    req.send(JSON.stringify(requestObj) );
}

floatingWindow.classList.toggle('hidden', true);