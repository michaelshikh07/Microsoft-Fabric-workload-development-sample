import { Stack } from "@fluentui/react";
import { TabValue, Divider, Label } from "@fluentui/react-components";
import React, { useMemo, useState } from "react";
import { useLocation, useParams } from "react-router-dom";
import { PageProps, ContextProps } from "src/App";
import { callItemDelete, callItemGet } from "../../controller/SampleWorkloadController";
import { WorkloadItem, ItemPayload } from "src/models/SampleWorkloadModel";
import { MrtaRibbon } from "../MrtaSampleWorkloadRibbon/MrtaSampleWorkloadRibbon";
import { convertGetItemResultToWorkloadItem } from "../../utils";


export function MrtaSampleWorkloadEditor(props: PageProps) {
    const pageContext = useParams<ContextProps>();
    const { pathname } = useLocation();
    const { workloadClient } = props;
    const [sampleItem, setSampleItem] = useState<WorkloadItem<ItemPayload>>(undefined);
    const [selectedTab, setSelectedTab] = useState<TabValue>("home");

    // load the existing Item (via its objectId)
    useMemo(() => loadDataFromUrl(pageContext, pathname), [pageContext, pathname]);

    async function loadDataFromUrl(pageContext: ContextProps, pathname: string): Promise<void> {
        if (pageContext.itemObjectId) {
            // for Edit scenario we get the itemObjectId and then load the item via the workloadClient SDK
            try {
                const getItemResult = await callItemGet(
                    pageContext.itemObjectId,
                    workloadClient
                );
                const item = convertGetItemResultToWorkloadItem<ItemPayload>(getItemResult);

                setSampleItem(item);

                // load extendedMetadata
                //const item1Metadata: Item1ClientMetadata = item.extendedMetdata.item1Metadata;
            } catch (error) {
                console.error(
                    `Error loading the Item (object ID:${pageContext.itemObjectId}`,
                    error
                );
                clearItemData();
            }
        } else {
            console.log(`non-editor context. Current Path: ${pathname}`);
            clearItemData();
        }
    }

    function getItemObjectId() {
        const params = useParams<ContextProps>();
        return sampleItem?.id || params.itemObjectId;
    }

    function clearItemData() {
        setSampleItem(undefined);
    }

    async function SaveItem() {
        // call ItemUpdate with the current payload contents
        // let payload: UpdateItemPayload = {
        //     item1Metadata: {
        //         lakehouse: selectedLakehouse,
        //         operand1: operand1,
        //         operand2: operand2,
        //         operator: operator,
        //     }
        // };

        // await callItemUpdate(sampleItem.id, payload, workloadClient);

        // setDirty(false);
    }

    async function DeleteItem() {
        if (sampleItem) {
            await callItemDelete(sampleItem.id, workloadClient);
        }
    }

    // HTML page contents
    return (
        <Stack className="editor">
            <MrtaRibbon
                {...props}
                saveItemCallback={SaveItem}
                isDeleteEnabled={sampleItem?.id !== undefined}
                deleteItemCallback={DeleteItem}
                itemObjectId={getItemObjectId()}
                onTabChange={setSelectedTab}
            />

            <Stack className="main">
                {selectedTab == "home" && (
                    <span>
                        <h2>Microsoft Real Time Analytics Item</h2>
                        {/* Crud item API usage example */}
                        <Divider alignContent="start">
                            <strong>{sampleItem ? "Item Details" : "Loading item ..."}</strong>
                        </Divider>
                        <div className="section">
                            {sampleItem && (
                                <Label><strong>WorkspaceId Id:</strong> {sampleItem?.workspaceId}</Label>
                            )}
                            {sampleItem &&
                             <Label><strong>Item Id:</strong> {sampleItem?.id}</Label>}
                            {sampleItem && (
                                <Label><strong>Item Display Name</strong> {sampleItem?.displayName}</Label>
                            )}
                            {sampleItem && (
                                <Label><strong>Item Description:</strong> {sampleItem?.description}</Label>
                            )}
                        </div>
                    </span>
                )}
            </Stack>
        </Stack>
    );
}