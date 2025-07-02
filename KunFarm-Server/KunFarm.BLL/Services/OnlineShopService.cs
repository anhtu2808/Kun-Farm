using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.Services
{
    public class OnlineShopService : IOnlineShopService
    {
        private readonly IOnlineShopRepository _onlineShopRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IPlayerStateRepository _playerStateRepository;

        public OnlineShopService(IOnlineShopRepository onlineShopRepository, IItemRepository itemRepository, IPlayerStateRepository playerStateRepository)
        {
            _onlineShopRepository = onlineShopRepository;
            _itemRepository = itemRepository;
            _playerStateRepository = playerStateRepository;
        }

        public async Task<ApiResponse<List<SellItemResponse>>> GetRandomSoldItem(int playerId)
        {
            List<SellItemResponse> responses = new List<SellItemResponse>();
            var shop = await _onlineShopRepository.GetAllWithoutSellerId(playerId);
            foreach (OnlineShopSlot slot in shop)
            {
                Item? item = await _itemRepository.GetItemById(slot.ItemId);
                SellItemResponse response = new()
                {
                    CollectableType = item.CollectableType,
                    Price = slot.BuyPrice,
                    Quantity = slot.Quantiy,
                    Icon = item.Icon,
                    Id = slot.Id,
                    CanBuy = slot.CanBuy
                };
                responses.Add(response);
            }
            return new ApiResponse<List<SellItemResponse>>
            {
                Code = 200,
                Data = responses,
                Message = "Random sold items retrieved successfully."
            };
        }

        public async Task<ApiResponse<bool>> BuyItem(int playerId, List<int> request)
        {
            // Lấy player state để kiểm tra tiền
            var playerState = await _playerStateRepository.GetByUserIdAsync(playerId);
            if (playerState == null)
            {
                return new ApiResponse<bool>
                {
                    Code = 404,
                    Data = false,
                    Message = "Player not found."
                };
            }

            int totalCost = 0;
            List<OnlineShopSlot> itemsToBuy = new List<OnlineShopSlot>();

            // Tính tổng chi phí và validate items
            foreach (var itemId in request)
            {
                var slot = await _onlineShopRepository.GetByIdAsync(itemId);
                if (slot == null || !slot.CanBuy)
                {
                    return new ApiResponse<bool>
                    {
                        Code = 400,
                        Data = false,
                        Message = $"Item with ID {itemId} cannot be bought or not found."
                    };
                }
                
                totalCost += slot.BuyPrice;
                itemsToBuy.Add(slot);
            }

            // Kiểm tra đủ tiền
            if (playerState.Money < totalCost)
            {
                return new ApiResponse<bool>
                {
                    Code = 400,
                    Data = false,
                    Message = $"Insufficient funds. Required: {totalCost}, Available: {playerState.Money}"
                };
            }

            // Trừ tiền từ player state
            bool moneyUpdated = await _playerStateRepository.UpdatePlayerMoneyAsync(playerId, playerState.Money - totalCost);
            if (!moneyUpdated)
            {
                return new ApiResponse<bool>
                {
                    Code = 500,
                    Data = false,
                    Message = "Failed to update player money."
                };
            }

            // Update shop items sau khi đã trừ tiền thành công
            foreach (var slot in itemsToBuy)
            {
                slot.BuyerId = playerId;
                slot.CanBuy = false;
                await _onlineShopRepository.UpdateAsync(slot);
            }

            return new ApiResponse<bool>
            {
                Code = 200,
                Data = true,
                Message = $"Items bought successfully. Total cost: {totalCost}G. Remaining balance: {playerState.Money - totalCost}G"
            };
        }

        public async Task<ApiResponse<SellItemResponse>> SellItem(int playerId, SellItemRequest request)
        {
            Item? item = await _itemRepository.GetItemByType(request.CollectableType);

            var slot = await _onlineShopRepository.AddAsync
                (
                new()
                {
                    CanBuy = true,
                    BuyPrice = request.Price,
                    ItemId = item.Id,
                    Quantiy = request.Quantity,
                    SellerId = playerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });

            SellItemResponse response = new()
            {
                CollectableType = item.CollectableType,
                Price = slot.BuyPrice,
                Quantity = slot.Quantiy,
                Icon = item.Icon,
                Id = slot.Id,
                CanBuy = slot.CanBuy
            };

            return new()
            {
                Code = 200,
                Data = response,
                Message = "Item sold successfully."
            };
        }

        /// <summary>
        /// Lấy tất cả items player đã bán (bao gồm cả đã bán và chưa bán)
        /// </summary>
        public async Task<ApiResponse<List<SellItemResponse>>> GetSoldItemsByPlayer(int playerId)
        {
            List<SellItemResponse> responses = new List<SellItemResponse>();
            var soldItems = await _onlineShopRepository.GetBySellerId(playerId);
            
            foreach (OnlineShopSlot slot in soldItems)
            {
                Item? item = await _itemRepository.GetItemById(slot.ItemId);
                if (item != null)
                {
                    SellItemResponse response = new()
                    {
                        CollectableType = item.CollectableType,
                        Price = slot.BuyPrice,
                        Quantity = slot.Quantiy,
                        Icon = item.Icon,
                        Id = slot.Id,
                        CanBuy = slot.CanBuy
                    };
                    responses.Add(response);
                }
            }
            
            return new ApiResponse<List<SellItemResponse>>
            {
                Code = 200,
                Data = responses,
                Message = $"Retrieved {responses.Count} sold items for player {playerId}."
            };
        }

        /// <summary>
        /// Claim tiền từ các items đã được bán (canBuy = false)
        /// </summary>
        public async Task<ApiResponse<bool>> ClaimMoney(int playerId, List<int> soldItemIds)
        {
            // Get player state để cộng tiền
            var playerState = await _playerStateRepository.GetByUserIdAsync(playerId);
            if (playerState == null)
            {
                return new ApiResponse<bool>
                {
                    Code = 404,
                    Data = false,
                    Message = "Player not found."
                };
            }

            int totalClaimAmount = 0;
            List<OnlineShopSlot> itemsToClaim = new List<OnlineShopSlot>();

            // Validate items và tính tổng tiền claim được
            foreach (var itemId in soldItemIds)
            {
                var slot = await _onlineShopRepository.GetByIdAsync(itemId);
                if (slot == null || slot.SellerId != playerId)
                {
                    return new ApiResponse<bool>
                    {
                        Code = 400,
                        Data = false,
                        Message = $"Item with ID {itemId} not found or not owned by player {playerId}."
                    };
                }

                // Chỉ claim items đã được bán (canBuy = false)
                if (!slot.CanBuy)
                {
                    totalClaimAmount += slot.BuyPrice;
                    itemsToClaim.Add(slot);
                }
                else
                {
                    return new ApiResponse<bool>
                    {
                        Code = 400,
                        Data = false,
                        Message = $"Item with ID {itemId} has not been sold yet (canBuy = true)."
                    };
                }
            }

            if (totalClaimAmount == 0)
            {
                return new ApiResponse<bool>
                {
                    Code = 400,
                    Data = false,
                    Message = "No money to claim."
                };
            }

            // Cộng tiền vào player state
            bool moneyUpdated = await _playerStateRepository.UpdatePlayerMoneyAsync(playerId, playerState.Money + totalClaimAmount);
            if (!moneyUpdated)
            {
                return new ApiResponse<bool>
                {
                    Code = 500,
                    Data = false,
                    Message = "Failed to update player money."
                };
            }

            // Xóa items đã claim khỏi database
            foreach (var slot in itemsToClaim)
            {
                await _onlineShopRepository.DeleteAsync(slot.Id);
            }

            return new ApiResponse<bool>
            {
                Code = 200,
                Data = true,
                Message = $"Successfully claimed {totalClaimAmount}G from {itemsToClaim.Count} sold items. New balance: {playerState.Money + totalClaimAmount}G"
            };
        }
    }
}
